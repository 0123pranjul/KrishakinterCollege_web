using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Repository
{
    public class MemberBlockRepository : IMemberBlockRepository
    {
        private readonly LibmanagementContext _context;

        public MemberBlockRepository(LibmanagementContext context)
        {
            _context = context;
        }

        public async Task<bool> IsBlockedAsync(string userType, int userId)
        {
            return await _context.LibMemberBlockLogs
                .AnyAsync(x => x.UserType == userType
                            && x.UserId == userId
                            && x.IsBlocked);
        }

        public async Task<LibMemberBlockLog?> GetActiveBlockAsync(string userType, int userId)
        {
            return await _context.LibMemberBlockLogs
                .Where(x => x.UserType == userType
                         && x.UserId == userId
                         && x.IsBlocked)
                .OrderByDescending(x => x.BlockedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<LibMemberBlockLog> BlockMemberAsync(LibMemberBlockLog blockLog)
        {
            _context.LibMemberBlockLogs.Add(blockLog);
            await _context.SaveChangesAsync();
            return blockLog;
        }

        public async Task<bool> UnblockMemberAsync(string userType, int userId, int unblockedBy, string reason)
        {
            var blocks = await _context.LibMemberBlockLogs
                .Where(x => x.UserType == userType
                         && x.UserId == userId
                         && x.IsBlocked)
                .ToListAsync();

            if (!blocks.Any()) return false;

            foreach (var block in blocks)
            {
                block.IsBlocked     = false;
                block.UnblockedBy   = unblockedBy;
                block.UnblockedAt   = DateTime.Now;
                block.UnblockReason = reason;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
