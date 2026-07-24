// payroll.js
var URLS = {}; var currentData = [];

function fmtRs(n) { return "\u20B9" + parseFloat(n || 0).toLocaleString("en-IN", { minimumFractionDigits: 2, maximumFractionDigits: 2 }); }

$(document).ready(function () {
    var el = document.getElementById('payrollUrls');
    if (el) {
        URLS.getAll        = el.dataset.getall;
        URLS.preview       = el.dataset.preview;
        URLS.generate      = el.dataset.generate;
        URLS.updateStatus  = el.dataset.updatestatus;
        URLS.getSlip       = el.dataset.getslip;
        URLS.getAllSlips    = el.dataset.getallslips;
        URLS.getPending    = el.dataset.getpending;
        URLS.getAdvances   = el.dataset.getadvances;
        URLS.addAdvance    = el.dataset.addadvance;
        URLS.deleteAdvance = el.dataset.deleteadvance;
        URLS.getEmployees  = el.dataset.getemployees;
    }
    var month = document.getElementById('selectedMonth').value;
    loadSalaryData(month);
    checkPendingAdvances(month);
    document.getElementById('selectedMonth').addEventListener('change', function () {
        loadSalaryData(this.value);
        checkPendingAdvances(this.value);
    });
});

function loadSalaryData(monthYear) {
    if (!monthYear) return;
    $.get(URLS.getAll, { monthYear: monthYear }, function (res) {
        if (res.data && res.data.length > 0) { currentData = res.data; renderTable(res.data, false); }
        else { clearTable(); }
    });
}

function checkPendingAdvances(monthYear) {
    if (!monthYear) return;
    $.get(URLS.getPending, { monthYear: monthYear }, function (res) {
        var banner = document.getElementById('advanceBanner');
        var txt    = document.getElementById('advanceBannerText');
        if (res.success && res.data && res.data.length > 0) {
            var total = res.data.reduce(function (s, a) { return s + parseFloat(a.amount || 0); }, 0);
            txt.textContent = res.data.length + ' employee(s) ke liye pending advance hai (Total: ' + fmtRs(total) + ')';
            banner.classList.remove('d-none');
        } else { banner.classList.add('d-none'); }
    });
}

function previewSalary() {
    var month = document.getElementById('selectedMonth').value;
    if (!month) { toastr.warning('Select month!'); return; }
    var btn = document.getElementById('btnGenerate');
    btn.disabled = true;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Loading...';
    $.get(URLS.preview, { monthYear: month }, function (res) {
        btn.disabled = false;
        btn.innerHTML = '<i class="fas fa-bolt me-1"></i>Generate Salary';
        if (res.success) {
            currentData = res.data;
            renderTable(res.data, true);
            if (res.pendingAdvances && res.pendingAdvances.length > 0) {
                var tot = res.pendingAdvances.reduce(function(s,a){ return s+parseFloat(a.amount||0); }, 0);
                toastr.warning('Preview ready! ' + res.pendingAdvances.length + ' advance(s) deduct honge (' + fmtRs(tot) + ')', 'Advance Alert');
            } else { toastr.info('Preview ready - click Generate to save!'); }
        } else { toastr.error(res.message); }
    });
}

function generateSalary(regenerate) {
    var month = document.getElementById('selectedMonth').value;
    if (!month) { toastr.warning('Select month!'); return; }
    var title  = regenerate ? 'Regenerate Salary?' : 'Generate Salary?';
    var text   = regenerate ? 'Existing salary will be deleted and recalculated!' : 'Generate salary for ' + month + '?';
    var color  = regenerate ? '#dc3545' : '#28a745';
    var btnTxt = regenerate ? 'Yes, Regenerate!' : 'Yes, Generate!';
    Swal.fire({ title: title, text: text, icon: 'question', showCancelButton: true, confirmButtonColor: color, confirmButtonText: btnTxt })
    .then(function (r) {
        if (!r.isConfirmed) return;
        var btn = document.getElementById('btnGenerate');
        btn.disabled = true;
        btn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Generating...';
        $.ajax({
            url: URLS.generate, type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ monthYear: month, regenerate: regenerate }),
            success: function (res) {
                btn.disabled = false;
                btn.innerHTML = '<i class="fas fa-bolt me-1"></i>Generate Salary';
                if (res.success) { Swal.fire('Done!', res.message, 'success'); loadSalaryData(month); checkPendingAdvances(month); }
                else { toastr.error(res.message); }
            },
            error: function () {
                btn.disabled = false;
                btn.innerHTML = '<i class="fas fa-bolt me-1"></i>Generate Salary';
                toastr.error('Error generating salary!');
            }
        });
    });
}

function renderTable(data, isPreview) {
    if (!data || !data.length) { clearTable(); return; }
    var html = '';
    var totBasic=0, totOT=0, totGross=0, totLwp=0, totAdv=0, totNet=0;
    for (var i = 0; i < data.length; i++) {
        var r = data[i];
        var lwpDed = parseFloat(r.lwpDeduction || 0);
        var advDed = parseFloat(r.advanceDeduction || 0);
        totBasic += parseFloat(r.basicSalary   || 0);
        totOT    += parseFloat(r.overtimeAmount || 0);
        totGross += parseFloat(r.grossSalary    || 0);
        totLwp   += lwpDed;
        totAdv   += advDed;
        totNet   += parseFloat(r.netSalary      || 0);
        var sCls  = isPreview ? 'status-generated' : 'status-' + (r.status || 'generated').toLowerCase();
        var sTxt  = isPreview ? 'Preview' : (r.status || '-');
        var hd    = r.halfDays || r.halfdays || 0;
        var hBadge = hd > 0 ? ' <small class="text-warning">(+' + hd + '&frac12;)</small>' : '';
        var lwpCls  = (r.lwpDays || 0) > 0 ? 'text-danger fw-bold' : '';
        var otCell  = parseFloat(r.overtimeAmount || 0) > 0 ? fmtRs(r.overtimeAmount) : '-';
        var lwpCell = lwpDed > 0 ? '<span class="text-danger">' + fmtRs(lwpDed) + '</span>' : '-';
        var advCell = advDed > 0 ? '<span class="text-warning fw-bold">' + fmtRs(advDed) + '</span>' : '-';
        var actions = isPreview ? '-'
            : '<div class="btn-group btn-group-sm">'
            + '<button class="btn btn-info"    onclick="viewSlip('    + r.id + ')" title="Salary Slip"><i class="fas fa-file-alt"></i></button>'
            + '<button class="btn btn-success" onclick="markStatus('  + r.id + ',\'Paid\')" title="Mark Paid"><i class="fas fa-check"></i></button>'
            + '<button class="btn btn-warning" onclick="markStatus('  + r.id + ',\'Hold\')" title="Hold"><i class="fas fa-pause"></i></button>'
            + '</div>';
        html += '<tr>'
            + '<td><strong>' + (r.employeeCode||'') + '</strong><br><small class="text-muted">' + (r.employeeName||'') + '</small></td>'
            + '<td class="text-center">'  + (r.payableDays||0) + '</td>'
            + '<td class="text-center">'  + (r.presentDays||0) + hBadge + '</td>'
            + '<td class="text-center">'  + (r.holidayDays||0) + '</td>'
            + '<td class="text-center">'  + (r.leaveDays||0)   + '</td>'
            + '<td class="text-center '   + lwpCls + '">' + (r.lwpDays||0) + '</td>'
            + '<td class="text-center">'  + (r.overtimeHours||0) + '</td>'
            + '<td class="text-end">'     + fmtRs(r.basicSalary) + '</td>'
            + '<td class="text-end text-success">' + otCell  + '</td>'
            + '<td class="text-end fw-bold">'      + fmtRs(r.grossSalary) + '</td>'
            + '<td class="text-end">'     + lwpCell + '</td>'
            + '<td class="text-end">'     + advCell + '</td>'
            + '<td class="text-end fw-bold text-success">' + fmtRs(r.netSalary) + '</td>'
            + '<td class="text-center"><span class="badge ' + sCls + ' px-2 py-1">' + sTxt + '</span></td>'
            + '<td class="text-center">'  + actions + '</td>'
            + '</tr>';
    }
    document.getElementById('salaryBody').innerHTML = html;
    document.getElementById('totBasic').textContent     = fmtRs(totBasic);
    document.getElementById('totOT').textContent        = totOT  > 0 ? fmtRs(totOT)  : '-';
    document.getElementById('totGross').textContent     = fmtRs(totGross);
    document.getElementById('totLwpDeduct').textContent = totLwp > 0 ? fmtRs(totLwp) : '-';
    document.getElementById('totAdvDeduct').textContent = totAdv > 0 ? fmtRs(totAdv) : '-';
    document.getElementById('totNet').textContent       = fmtRs(totNet);
    document.getElementById('totalRow').style.display   = '';
    document.getElementById('sumEmp').textContent       = data.length;
    document.getElementById('sumGross').textContent     = '\u20B9' + Math.round(totGross / 1000) + 'K';
    document.getElementById('sumNet').textContent       = '\u20B9' + Math.round(totNet   / 1000) + 'K';
    document.getElementById('summaryCards').style.cssText = 'display:flex!important';
    updateRecordCount(data);
}

function clearTable() {
    document.getElementById('salaryBody').innerHTML =
        '<tr><td colspan="15" class="text-center text-muted py-4"><i class="fas fa-info-circle me-2"></i>No salary data. Select month and click Preview or Generate.</td></tr>';
    document.getElementById('totalRow').style.display = 'none';
    document.getElementById('summaryCards').style.display = 'none';
    currentData = [];
}

function updateRecordCount(data) {
    var paid = data.filter(function(r){ return (r.status||'').toLowerCase() === 'paid'; }).length;
    document.getElementById('recordCount').textContent = 'Total: ' + data.length + ' | Paid: ' + paid + ' | Pending: ' + (data.length - paid);
}

function filterTable() {
    var status = document.getElementById('filterStatus').value.toLowerCase();
    if (!currentData.length) return;
    var filtered = status ? currentData.filter(function(r){ return (r.status||'generated').toLowerCase() === status; }) : currentData;
    renderTable(filtered, false);
}

function markStatus(id, status) {
    $.ajax({
        url: URLS.updateStatus, type: 'POST', contentType: 'application/json',
        data: JSON.stringify({ id: id, status: status, monthYear: '' }),
        success: function (r) {
            if (r.success) { toastr.success(r.message); loadSalaryData(document.getElementById('selectedMonth').value); }
            else { toastr.error(r.message); }
        }
    });
}

function markAllPaid() {
    var month = document.getElementById('selectedMonth').value;
    if (!month) { toastr.warning('Select month!'); return; }
    Swal.fire({ title: 'Mark All as Paid?', text: 'All salaries for ' + month + ' will be marked Paid!', icon: 'question', showCancelButton: true, confirmButtonColor: '#28a745', confirmButtonText: 'Yes, Mark All Paid!' })
    .then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({
            url: URLS.updateStatus, type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ id: 0, status: 'Paid', monthYear: month }),
            success: function (res) {
                if (res.success) { toastr.success(res.message); loadSalaryData(month); }
                else { toastr.error(res.message); }
            }
        });
    });
}

//  Download All Slips 
function downloadAllSlips() {
    var month = document.getElementById('selectedMonth').value;
    if (!month) { toastr.warning('Select month!'); return; }
    var btn = document.getElementById('btnDownloadAll');
    btn.disabled = true;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Loading...';
    $.get(URLS.getAllSlips, { monthYear: month }, function (res) {
        btn.disabled = false;
        btn.innerHTML = '<i class="fas fa-download me-1"></i>Download All Slips';
        if (!res.success) { toastr.error(res.message || 'No data found!'); return; }
        var parts = month.split('-');
        var yr   = parts[0];
        var mn   = parts[1];
        var mName = new Date(parseInt(yr), parseInt(mn) - 1).toLocaleString('default', { month: 'long' });
        var html = '<!DOCTYPE html><html><head><meta charset="UTF-8"><title>Salary Slips - ' + mName + ' ' + yr + '</title><style>'
            + 'body{font-family:Arial,sans-serif;font-size:13px}'
            + '.slip{width:750px;margin:20px auto;border:2px solid #333;padding:20px;page-break-after:always}'
            + '.slip:last-child{page-break-after:auto}'
            + 'h3{text-align:center;margin:0}.sub{text-align:center;color:#555;margin-bottom:15px}'
            + 'table{width:100%;border-collapse:collapse;margin-bottom:10px}'
            + 'td,th{border:1px solid #ccc;padding:5px 8px}'
            + '.nb td{border:none;padding:3px 8px}'
            + '.rt{text-align:right}.tr-tot{background:#d4edda;font-weight:bold}'
            + '.deduct{color:#dc3545}.earn{color:#28a745}'
            + '</style></head><body>';
        for (var i = 0; i < res.data.length; i++) {
            var s = res.data[i];
            html += '<div class="slip">'
                + '<h3>SALARY SLIP</h3><div class="sub">' + mName + ' ' + yr + '</div>'
                + '<table><tr>'
                + '<td class="nb" width="50%"><table class="nb">'
                + '<tr><td style="color:#666">Emp Code</td><td><b>' + (s.employeeCode||'') + '</b></td></tr>'
                + '<tr><td style="color:#666">Name</td><td><b>' + (s.employeeName||'') + '</b></td></tr>'
                + '<tr><td style="color:#666">Designation</td><td>' + (s.designation||'-') + '</td></tr>'
                + '<tr><td style="color:#666">Department</td><td>' + (s.department||'-') + '</td></tr>'
                + '</table></td>'
                + '<td class="nb" width="50%"><table class="nb">'
                + '<tr><td style="color:#666">Month</td><td><b>' + mName + ' ' + yr + '</b></td></tr>'
                + '<tr><td style="color:#666">Payable Days</td><td>' + s.payableDays + '</td></tr>'
                + '<tr><td style="color:#666">Status</td><td><b>' + (s.status||'-') + '</b></td></tr>'
                + '</table></td></tr></table>'
                + '<table><thead><tr style="background:#333;color:#fff">'
                + '<th>Attendance</th><th class="rt">Days</th><th>Earnings / Deductions</th><th class="rt">Amount</th>'
                + '</tr></thead><tbody>'
                + '<tr><td>Present Days</td><td class="rt">'    + s.presentDays + '</td><td>Basic Salary</td><td class="rt">'   + fmtRs(s.basicSalary)  + '</td></tr>'
                + '<tr><td>Holiday Days</td><td class="rt">'    + s.holidayDays + '</td><td class="earn">+ OT Amount</td><td class="rt earn">' + fmtRs(s.overtimeAmount) + '</td></tr>'
                + '<tr><td>Leave Days</td><td class="rt">'      + s.leaveDays   + '</td><td><b>Gross Salary</b></td><td class="rt"><b>' + fmtRs(s.grossSalary) + '</b></td></tr>'
                + '<tr><td>LWP Days</td><td class="rt deduct">' + s.lwpDays     + '</td><td class="deduct">- LWP Deduction</td><td class="rt deduct">- ' + fmtRs(s.lwpDeduction||0) + '</td></tr>'
                + '<tr><td>OT Hours</td><td class="rt">'        + s.overtimeHours + '</td><td class="deduct">- Advance Deduction</td><td class="rt deduct">- ' + fmtRs(s.advanceDeduction||0) + '</td></tr>'
                + '<tr class="tr-tot"><td colspan="2"></td><td>NET SALARY</td><td class="rt">' + fmtRs(s.netSalary) + '</td></tr>'
                + '</tbody></table>'
                + '<div style="text-align:center;color:#888;font-size:11px;margin-top:8px">Generated: ' + new Date().toLocaleDateString('en-IN') + '</div>'
                + '</div>';
        }
        html += '</body></html>';
        var blob = new Blob([html], { type: 'text/html;charset=utf-8' });
        var url  = URL.createObjectURL(blob);
        var a    = document.createElement('a');
        a.href = url; a.download = 'SalarySlips_' + month + '.html';
        document.body.appendChild(a); a.click();
        document.body.removeChild(a); URL.revokeObjectURL(url);
        toastr.success(res.data.length + ' salary slips downloaded! Browser me open karke Ctrl+P se print karein.', 'Done', { timeOut: 5000 });
    }).fail(function () {
        btn.disabled = false;
        btn.innerHTML = '<i class="fas fa-download me-1"></i>Download All Slips';
        toastr.error('Download failed!');
    });
}

//  Advance Modal 
function openAdvanceModal() {
    $.get(URLS.getEmployees, function (res) {
        if (res.success && res.data) {
            var opts = '<option value="">-- Select Employee --</option>';
            var fOpts = '<option value="">All Employees</option>';
            for (var i = 0; i < res.data.length; i++) {
                var e = res.data[i];
                var txt = e.employeeCode + ' - ' + e.name;
                opts  += '<option value="' + e.id + '">' + txt + '</option>';
                fOpts += '<option value="' + e.id + '">' + txt + '</option>';
            }
            document.getElementById('advEmpId').innerHTML    = opts;
            document.getElementById('advFilterEmp').innerHTML = fOpts;
        }
    });
    loadAdvances();
    new bootstrap.Modal(document.getElementById('advanceModal')).show();
}

function loadAdvances() {
    var empId  = document.getElementById('advFilterEmp').value;
    var status = document.getElementById('advFilterStatus').value;
    $.get(URLS.getAdvances, { employeeId: empId || 0, status: status }, function (res) {
        if (!res.success) return;
        var html = '';
        var pendingTotal = 0;
        if (!res.data || !res.data.length) {
            html = '<tr><td colspan="7" class="text-center text-muted py-3">No advance records found.</td></tr>';
        } else {
            for (var i = 0; i < res.data.length; i++) {
                var a = res.data[i];
                var isPending = a.status === 'Pending';
                if (isPending) pendingTotal += parseFloat(a.amount || 0);
                var cls = isPending ? 'adv-pending' : 'adv-deducted';
                var delBtn = isPending
                    ? '<button class="btn btn-danger btn-sm" onclick="deleteAdvance(' + a.id + ')" title="Delete"><i class="fas fa-trash"></i></button>'
                    : '<span class="text-success"><i class="fas fa-check-circle"></i></span>';
                var deductFrom = a.deductFromMonth || '<em class="text-muted">Next Salary</em>';
                html += '<tr>'
                    + '<td><strong>' + a.employeeCode + '</strong><br><small>' + a.employeeName + '</small></td>'
                    + '<td>' + a.advanceDate + '</td>'
                    + '<td class="text-end fw-bold">' + fmtRs(a.amount) + '</td>'
                    + '<td>' + (a.reason||'-') + '</td>'
                    + '<td class="text-center">' + deductFrom + '</td>'
                    + '<td class="text-center"><span class="badge ' + cls + ' px-2 py-1">' + a.status + '</span></td>'
                    + '<td class="text-center">' + delBtn + '</td>'
                    + '</tr>';
            }
        }
        document.getElementById('advanceTableBody').innerHTML = html;
        document.getElementById('advTotalPending').textContent = pendingTotal > 0 ? fmtRs(pendingTotal) : '-';
    });
}

function saveAdvance() {
    var empId  = document.getElementById('advEmpId').value;
    var amount = parseFloat(document.getElementById('advAmount').value);
    var date   = document.getElementById('advDate').value;
    var month  = document.getElementById('advDeductMonth').value;
    var reason = document.getElementById('advReason').value;
    if (!empId)          { toastr.warning('Employee select karein!'); return; }
    if (!amount || amount <= 0) { toastr.warning('Valid amount enter karein!'); return; }
    $.ajax({
        url: URLS.addAdvance, type: 'POST', contentType: 'application/json',
        data: JSON.stringify({ employeeId: parseInt(empId), amount: amount, advanceDate: date || null, deductFromMonth: month || null, reason: reason }),
        success: function (r) {
            if (r.success) {
                toastr.success(r.message);
                document.getElementById('advEmpId').value = '';
                document.getElementById('advAmount').value = '';
                document.getElementById('advDeductMonth').value = '';
                document.getElementById('advReason').value = '';
                loadAdvances();
                checkPendingAdvances(document.getElementById('selectedMonth').value);
            } else { toastr.error(r.message); }
        },
        error: function () { toastr.error('Error saving advance!'); }
    });
}

function deleteAdvance(id) {
    Swal.fire({ title: 'Delete Advance?', text: 'Yeh advance record delete ho jayega!', icon: 'warning', showCancelButton: true, confirmButtonColor: '#dc3545', confirmButtonText: 'Yes, Delete!' })
    .then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({
            url: URLS.deleteAdvance, type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ id: id }),
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    loadAdvances();
                    checkPendingAdvances(document.getElementById('selectedMonth').value);
                } else { toastr.error(res.message); }
            }
        });
    });
}

//  Salary Slip (Single) 
function viewSlip(id) {
    $.get(URLS.getSlip, { id: id }, function (res) {
        if (!res.success) { toastr.error('Slip not found!'); return; }
        var s = res.data;
        var parts = (s.monthYear || '').split('-');
        var yr    = parts[0] || '';
        var mn    = parts[1] || '';
        var mName = mn ? new Date(parseInt(yr), parseInt(mn) - 1).toLocaleString('default', { month: 'long' }) : '';
        var advDed = parseFloat(s.advanceDeduction || 0);
        var lwpDed = parseFloat(s.lwpDeduction || (parseFloat(s.deductions || 0) - advDed));
        var advRow = advDed > 0
            ? '<tr><td class="text-warning">Advance Deduction</td><td class="text-end text-warning fw-bold">- ' + fmtRs(advDed) + '</td></tr>'
            : '';
        document.getElementById('slipContent').innerHTML =
            '<div id="printArea">'
            + '<div class="text-center border-bottom pb-2 mb-3"><h4 class="mb-0">SALARY SLIP</h4><div class="text-muted">' + mName + ' ' + yr + '</div></div>'
            + '<div class="row mb-3">'
            + '<div class="col-6"><table class="table table-sm table-borderless">'
            + '<tr><td class="text-muted">Employee Code</td><td><strong>' + (s.employeeCode||'') + '</strong></td></tr>'
            + '<tr><td class="text-muted">Name</td><td><strong>' + (s.employeeName||'') + '</strong></td></tr>'
            + '<tr><td class="text-muted">Designation</td><td>' + (s.designation||'-') + '</td></tr>'
            + '<tr><td class="text-muted">Department</td><td>' + (s.department||'-') + '</td></tr>'
            + '</table></div>'
            + '<div class="col-6"><table class="table table-sm table-borderless">'
            + '<tr><td class="text-muted">Month</td><td><strong>' + mName + ' ' + yr + '</strong></td></tr>'
            + '<tr><td class="text-muted">Payable Days</td><td>' + s.payableDays + '</td></tr>'
            + '<tr><td class="text-muted">Status</td><td><span class="badge bg-success">' + (s.status||'-') + '</span></td></tr>'
            + '</table></div></div>'
            + '<div class="row">'
            + '<div class="col-6"><h6 class="bg-light p-2 rounded">Attendance Summary</h6>'
            + '<table class="table table-sm table-bordered">'
            + '<tr><td>Present Days</td><td class="text-end fw-bold">' + s.presentDays + '</td></tr>'
            + '<tr><td>Holiday Days</td><td class="text-end">' + s.holidayDays + '</td></tr>'
            + '<tr><td>Leave Days</td><td class="text-end">' + s.leaveDays + '</td></tr>'
            + '<tr><td>LWP Days</td><td class="text-end text-danger">' + s.lwpDays + '</td></tr>'
            + '<tr><td>OT Hours</td><td class="text-end text-success">' + s.overtimeHours + '</td></tr>'
            + '</table></div>'
            + '<div class="col-6"><h6 class="bg-light p-2 rounded">Salary Breakdown</h6>'
            + '<table class="table table-sm table-bordered">'
            + '<tr><td>Basic Salary</td><td class="text-end">' + fmtRs(s.basicSalary) + '</td></tr>'
            + '<tr><td class="text-success">+ OT Amount</td><td class="text-end text-success">+ ' + fmtRs(s.overtimeAmount) + '</td></tr>'
            + '<tr><td><strong>Gross Salary</strong></td><td class="text-end fw-bold">' + fmtRs(s.grossSalary) + '</td></tr>'
            + '<tr><td class="text-danger">LWP Deduction</td><td class="text-end text-danger">- ' + fmtRs(lwpDed) + '</td></tr>'
            + advRow
            + '<tr class="table-success"><td><strong>Net Salary</strong></td><td class="text-end fw-bold fs-5">' + fmtRs(s.netSalary) + '</td></tr>'
            + '</table></div></div>'
            + '<div class="text-center text-muted small mt-2">Generated on ' + new Date().toLocaleDateString('en-IN') + '</div>'
            + '</div>';
        new bootstrap.Modal(document.getElementById('slipModal')).show();
    });
}

function printSlip() { window.print(); }

//  View Single Salary Slip 
function viewSlip(id) {
    $.get(URLS.getSlip, { id: id }, function (res) {
        if (!res.success) { toastr.error('Slip not found!'); return; }
        var s = res.data;
        var parts = (s.monthYear || '').split('-');
        var yr = parts[0] || '';
        var mn = parts[1] || '';
        var mName = mn ? new Date(parseInt(yr), parseInt(mn) - 1).toLocaleString('default', { month: 'long' }) : '';
        var advDed = parseFloat(s.advanceDeduction || 0);
        var lwpDed = parseFloat(s.lwpDeduction || 0);
        var advRow = advDed > 0
            ? '<tr><td class="text-warning">Advance Deduction</td><td class="text-end text-warning fw-bold">- ' + fmtRs(advDed) + '</td></tr>'
            : '';
        var html = '<div id="printArea">'
            + '<div class="text-center border-bottom pb-2 mb-3"><h4 class="mb-0">SALARY SLIP</h4><div class="text-muted">' + mName + ' ' + yr + '</div></div>'
            + '<div class="row mb-3">'
            + '<div class="col-6"><table class="table table-sm table-borderless">'
            + '<tr><td class="text-muted">Employee Code</td><td><strong>' + (s.employeeCode||'') + '</strong></td></tr>'
            + '<tr><td class="text-muted">Name</td><td><strong>' + (s.employeeName||'') + '</strong></td></tr>'
            + '<tr><td class="text-muted">Designation</td><td>' + (s.designation||'-') + '</td></tr>'
            + '<tr><td class="text-muted">Department</td><td>' + (s.department||'-') + '</td></tr>'
            + '</table></div>'
            + '<div class="col-6"><table class="table table-sm table-borderless">'
            + '<tr><td class="text-muted">Month</td><td><strong>' + mName + ' ' + yr + '</strong></td></tr>'
            + '<tr><td class="text-muted">Payable Days</td><td>' + s.payableDays + '</td></tr>'
            + '<tr><td class="text-muted">Status</td><td><span class="badge bg-success">' + (s.status||'-') + '</span></td></tr>'
            + '</table></div>'
            + '</div>'
            + '<div class="row">'
            + '<div class="col-6"><h6 class="bg-light p-2 rounded">Attendance Summary</h6>'
            + '<table class="table table-sm table-bordered">'
            + '<tr><td>Present Days</td><td class="text-end fw-bold">' + s.presentDays + '</td></tr>'
            + '<tr><td>Holiday Days</td><td class="text-end">'        + s.holidayDays + '</td></tr>'
            + '<tr><td>Leave Days</td><td class="text-end">'          + s.leaveDays   + '</td></tr>'
            + '<tr><td>LWP Days</td><td class="text-end text-danger">'+ s.lwpDays     + '</td></tr>'
            + '<tr><td>OT Hours</td><td class="text-end text-success">'+ s.overtimeHours + '</td></tr>'
            + '</table></div>'
            + '<div class="col-6"><h6 class="bg-light p-2 rounded">Salary Breakdown</h6>'
            + '<table class="table table-sm table-bordered">'
            + '<tr><td>Basic Salary</td><td class="text-end">'                 + fmtRs(s.basicSalary)    + '</td></tr>'
            + '<tr><td>OT Amount</td><td class="text-end text-success">+ '     + fmtRs(s.overtimeAmount) + '</td></tr>'
            + '<tr><td><strong>Gross Salary</strong></td><td class="text-end fw-bold">' + fmtRs(s.grossSalary) + '</td></tr>'
            + '<tr><td class="text-danger">LWP Deduction</td><td class="text-end text-danger">- ' + fmtRs(lwpDed) + '</td></tr>'
            + advRow
            + '<tr class="table-success"><td><strong>Net Salary</strong></td><td class="text-end fw-bold fs-5">' + fmtRs(s.netSalary) + '</td></tr>'
            + '</table></div>'
            + '</div>'
            + '<div class="text-center text-muted small mt-2">Generated on ' + new Date().toLocaleDateString('en-IN') + '</div>'
            + '</div>';
        document.getElementById('slipContent').innerHTML = html;
        new bootstrap.Modal(document.getElementById('slipModal')).show();
    });
}

function printSlip() { window.print(); }
