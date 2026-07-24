// ================================================================
//  sessionRollover.js  —  Session Promotion Wizard State & Logic
// ================================================================

// ── Wizard State ─────────────────────────────────────────────────
var WZ = {
    currentStep       : 1,
    sourceSessionId   : 0,
    targetSessionId   : 0,
    allClasses        : [],
    // Step 2
    allClassSections  : [],   // from server
    selectedCSIds     : [],   // int[]
    // Step 3
    allTimetable      : [],   // from server
    selectedTTIds     : [],   // int[]
    // Step 4
    allStudents       : [],   // from server
    studentActions    : {},   // { studentId: { action, targetClassId, targetSectionId, ... } }
};

// ── Init ─────────────────────────────────────────────────────────
$(document).ready(function () {
    loadSessions();
    $.get(URLS.getClasses, function (data) { WZ.allClasses = data; });
});

function loadSessions() {
    $.get(URLS.getSessions, function (data) {
        data.forEach(function (s) {
            var opt = '<option value="' + s.sessionId + '">' + s.sessionName + '</option>';
            $('#srcSession, #tgtSession').append(opt);
        });
    });
}

// ── Step Navigation ───────────────────────────────────────────────
function goToStep(n) {
    $('.step-card').removeClass('active');
    $('#step-' + n).addClass('active');
    for (var i = 1; i <= 5; i++) {
        var si = $('#si-' + i);
        si.removeClass('active done');
        if (i < n)  si.addClass('done');
        if (i === n) si.addClass('active');
    }
    WZ.currentStep = n;
    window.scrollTo(0, 0);
}

// ── STEP 1 ────────────────────────────────────────────────────────
function step1Next() {
    var src = parseInt($('#srcSession').val());
    var tgt = parseInt($('#tgtSession').val());
    $('#step1-error').hide();

    if (!src || !tgt) {
        $('#step1-error').text('Dono sessions select karo.').show(); return;
    }
    if (src === tgt) {
        $('#step1-error').text('Source aur Target session alag hone chahiye.').show(); return;
    }
    WZ.sourceSessionId = src;
    WZ.targetSessionId = tgt;
    loadStep2();
    goToStep(2);
}

// ── STEP 2: ClassSection ──────────────────────────────────────────
function loadStep2() {
    $('#cs-list').html('<div class="text-center py-5 text-muted"><i class="fas fa-spinner fa-spin fa-2x"></i></div>');
    $.get(URLS.getClassSections, { sourceSessionId: WZ.sourceSessionId }, function (res) {
        WZ.allClassSections = res.data || [];
        // restore previous selection or select all
        if (WZ.selectedCSIds.length === 0)
            WZ.selectedCSIds = WZ.allClassSections.map(function (x) { return x.id; });
        renderCS();
    });
}

function renderCS() {
    if (!WZ.allClassSections.length) {
        $('#cs-list').html('<div class="alert alert-info m-3">Source session mein koi ClassSection mapping nahi mili. Next step par jaayein.</div>');
        return;
    }
    // group by class
    var groups = {};
    WZ.allClassSections.forEach(function (x) {
        if (!groups[x.classId]) groups[x.classId] = { className: x.className, items: [] };
        groups[x.classId].items.push(x);
    });
    var html = '<div class="p-3">';
    Object.keys(groups).forEach(function (cid) {
        var g = groups[cid];
        html += '<div class="mb-3"><h6 class="fw-bold text-primary mb-2"><i class="fas fa-chalkboard me-1"></i>' + g.className + '</h6><div class="d-flex flex-wrap gap-2">';
        g.items.forEach(function (item) {
            var chk = WZ.selectedCSIds.indexOf(item.id) > -1 ? 'checked' : '';
            html += '<div class="form-check form-check-inline border rounded px-3 py-2" style="background:#f8fafc">'
                  + '<input class="form-check-input cs-chk" type="checkbox" id="cs_' + item.id + '" value="' + item.id + '" ' + chk + ' onchange="toggleCS(' + item.id + ')">'
                  + '<label class="form-check-label" for="cs_' + item.id + '">' + item.sectionName + '</label>'
                  + '</div>';
        });
        html += '</div></div>';
    });
    html += '</div>';
    $('#cs-list').html(html);
}

function toggleCS(id) {
    var idx = WZ.selectedCSIds.indexOf(id);
    if (idx > -1) WZ.selectedCSIds.splice(idx, 1);
    else WZ.selectedCSIds.push(id);
}
function selectAllCS()   { WZ.selectedCSIds = WZ.allClassSections.map(function (x) { return x.id; }); renderCS(); }
function deselectAllCS() { WZ.selectedCSIds = []; renderCS(); }

function step2Next() { loadStep3(); goToStep(3); }

// ── STEP 3: Timetable ─────────────────────────────────────────────
function loadStep3() {
    $('#tt-list').html('<div class="text-center py-5 text-muted"><i class="fas fa-spinner fa-spin fa-2x"></i></div>');
    if (!WZ.selectedCSIds.length) {
        $('#tt-list').html('<div class="alert alert-info m-3">Koi ClassSection select nahi kiya. Timetable entries nahi layi jayengi.</div>');
        return;
    }
    var params = { sourceSessionId: WZ.sourceSessionId };
    WZ.selectedCSIds.forEach(function (id, i) { params['classSectionIds[' + i + ']'] = id; });
    $.get(URLS.getTimetable, params, function (res) {
        WZ.allTimetable = res.data || [];
        if (WZ.selectedTTIds.length === 0)
            WZ.selectedTTIds = WZ.allTimetable.map(function (x) { return x.timeTableId; });
        renderTT();
    });
}

function renderTT() {
    if (!WZ.allTimetable.length) {
        $('#tt-list').html('<div class="alert alert-info m-3">In ClassSections ke liye koi timetable entry nahi mili.</div>');
        return;
    }
    var groups = {};
    WZ.allTimetable.forEach(function (x) {
        var key = x.classId + '_' + x.sectionId;
        if (!groups[key]) groups[key] = { label: x.className + ' — ' + x.sectionName, items: [] };
        groups[key].items.push(x);
    });
    var days = ['', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    var html = '<div class="p-3">';
    Object.keys(groups).forEach(function (key) {
        var g = groups[key];
        html += '<h6 class="fw-bold text-primary mb-2"><i class="fas fa-calendar-alt me-1"></i>' + g.label + '</h6>'
              + '<div class="table-responsive mb-3"><table class="table table-sm table-bordered" style="font-size:0.8rem">'
              + '<thead class="table-light"><tr><th></th><th>Day</th><th>Period</th><th>Subject</th><th>Teacher</th></tr></thead><tbody>';
        g.items.forEach(function (item) {
            var chk = WZ.selectedTTIds.indexOf(item.timeTableId) > -1 ? 'checked' : '';
            html += '<tr>'
                  + '<td class="text-center"><input type="checkbox" class="tt-chk" value="' + item.timeTableId + '" ' + chk + ' onchange="toggleTT(' + item.timeTableId + ')"></td>'
                  + '<td>' + days[item.dayOfWeek] + '</td>'
                  + '<td>' + item.periodName + '</td>'
                  + '<td>' + item.subjectName + '</td>'
                  + '<td>' + item.teacherName + '</td>'
                  + '</tr>';
        });
        html += '</tbody></table></div>';
    });
    html += '</div>';
    $('#tt-list').html(html);
}

function toggleTT(id) {
    var idx = WZ.selectedTTIds.indexOf(id);
    if (idx > -1) WZ.selectedTTIds.splice(idx, 1);
    else WZ.selectedTTIds.push(id);
}
function selectAllTT()   { WZ.selectedTTIds = WZ.allTimetable.map(function (x) { return x.timeTableId; }); renderTT(); }
function deselectAllTT() { WZ.selectedTTIds = []; renderTT(); }

function step3Next() { loadStep4(); goToStep(4); }

// ── STEP 4: Students ──────────────────────────────────────────────
function loadStep4() {
    $('#student-list').html('<div class="text-center py-5 text-muted"><i class="fas fa-spinner fa-spin fa-2x"></i></div>');
    $.get(URLS.getStudents, { sourceSessionId: WZ.sourceSessionId }, function (res) {
        WZ.allStudents = res.data || [];
        // init default actions
        WZ.allStudents.forEach(function (s) {
            if (!WZ.studentActions[s.studentId]) {
                WZ.studentActions[s.studentId] = {
                    action: 'promote', targetClassId: null, targetSectionId: null,
                    retentionRemarks: '', exitReason: '', exitDate: '', exitRemarks: ''
                };
            }
        });
        renderStudents();
    });
}

function renderStudents() {
    if (!WZ.allStudents.length) {
        $('#student-list').html('<div class="alert alert-info m-3">Source session mein koi enrolled student nahi mila.</div>');
        return;
    }
    var groups = {};
    WZ.allStudents.forEach(function (s) {
        var key = s.classId + '_' + s.sectionId;
        if (!groups[key]) groups[key] = { className: s.className, sectionName: s.sectionName, classId: s.classId, items: [] };
        groups[key].items.push(s);
    });

    var html = '';
    Object.keys(groups).forEach(function (key) {
        var g = groups[key];
        html += '<div class="border-bottom">'
              + '<div class="d-flex align-items-center px-3 py-2 bg-light gap-3">'
              + '<span class="fw-bold text-primary"><i class="fas fa-chalkboard me-1"></i>' + g.className + ' — ' + g.sectionName + '</span>'
              + '<span class="badge bg-secondary">' + g.items.length + ' students</span>'
              + '<div class="ms-auto d-flex align-items-center gap-2">'
              + '<span class="small text-muted">Bulk:</span>'
              + '<select class="form-select form-select-sm" style="width:160px" onchange="bulkAction(\'' + key + '\', this.value)">'
              + '<option value="">-- Bulk Action --</option>'
              + '<option value="promote">Promote All</option>'
              + '<option value="failed">Failed All</option>'
              + '<option value="retained">Retain All (Other)</option>'
              + '<option value="passout">Passout All</option>'
              + '<option value="leftschool">Left School All</option>'
              + '</select></div></div>'
              + '<div class="table-responsive"><table class="table table-sm student-table mb-0">'
              + '<thead class="table-dark"><tr>'
              + '<th>Student</th><th>Roll No</th><th>Action</th><th>Target Section</th><th>Details</th><th></th>'
              + '</tr></thead><tbody>';

        g.items.forEach(function (s) {
            var sa = WZ.studentActions[s.studentId];
            html += buildStudentRow(s, sa);
        });
        html += '</tbody></table></div></div>';
    });
    $('#student-list').html(html);
}

function buildStudentRow(s, sa) {
    var actions = [
        { val: 'promote',    label: 'Promote',        badge: 'bg-success' },
        { val: 'failed',     label: 'Failed',          badge: 'bg-danger' },
        { val: 'retained',   label: 'Retain (Other)',  badge: 'bg-warning text-dark' },
        { val: 'passout',    label: 'Passout',         badge: 'bg-primary' },
        { val: 'leftschool', label: 'Left School',     badge: 'bg-secondary' }
    ];
    var sel = '<select class="form-select form-select-sm action-select" onchange="changeAction(' + s.studentId + ', this.value)">';
    actions.forEach(function (a) {
        sel += '<option value="' + a.val + '"' + (sa.action === a.val ? ' selected' : '') + '>' + a.label + '</option>';
    });
    sel += '</select>';

    var tgtSec = '<div id="tgt_' + s.studentId + '">—</div>';
    var details = '<div id="det_' + s.studentId + '"></div>';

    return '<tr id="row_' + s.studentId + '">'
         + '<td><strong>' + s.studentName + '</strong></td>'
         + '<td>' + (s.rollNo || '—') + '</td>'
         + '<td>' + sel + '</td>'
         + '<td>' + tgtSec + '</td>'
         + '<td>' + details + '</td>'
         + '<td><button class="btn btn-sm btn-outline-dark" title="History" onclick="showHistory(' + s.studentId + ')"><i class="fas fa-history"></i></button></td>'
         + '</tr>';
}

function changeAction(studentId, action) {
    WZ.studentActions[studentId].action = action;
    WZ.studentActions[studentId].targetClassId   = null;
    WZ.studentActions[studentId].targetSectionId = null;

    var s = WZ.allStudents.find(function (x) { return x.studentId === studentId; });
    var tgtDiv = $('#tgt_' + studentId);
    var detDiv = $('#det_' + studentId);
    detDiv.html('');
    tgtDiv.html('—');

    if (action === 'promote') {
        $.get(URLS.getNextClass, { currentClassId: s.classId }, function (res) {
            if (res.isHighest) {
                tgtDiv.html('<span class="text-warning small">Highest class!</span>');
            } else {
                WZ.studentActions[studentId].targetClassId = res.nextClass.classId;
                loadSectionDropdown(studentId, res.nextClass.classId, 'tgt_' + studentId, res.nextClass.className);
            }
        });
    } else if (action === 'failed' || action === 'retained') {
        WZ.studentActions[studentId].targetClassId = s.classId;
        loadSectionDropdown(studentId, s.classId, 'tgt_' + studentId, s.className);
        if (action === 'failed') {
            detDiv.html('<input type="text" class="form-control form-control-sm mt-1" placeholder="Fail remarks (optional)" oninput="WZ.studentActions[' + studentId + '].retentionRemarks=this.value">');
        } else {
            detDiv.html('<input type="text" class="form-control form-control-sm mt-1" placeholder="Retention reason (required)" oninput="WZ.studentActions[' + studentId + '].retentionRemarks=this.value">');
        }
    } else if (action === 'leftschool') {
        detDiv.html(
            '<div class="sub-form mt-1">'
          + '<select class="form-select form-select-sm mb-1" onchange="WZ.studentActions[' + studentId + '].exitReason=this.value">'
          + '<option value="">-- Exit Reason --</option>'
          + '<option>Transfer Certificate (TC)</option><option>Dropout</option><option>Migration</option><option>Expulsion</option><option>Other</option>'
          + '</select>'
          + '<input type="date" class="form-control form-control-sm mb-1" value="' + new Date().toISOString().split('T')[0] + '" onchange="WZ.studentActions[' + studentId + '].exitDate=this.value">'
          + '<input type="text" class="form-control form-control-sm" placeholder="Remarks (optional)" oninput="WZ.studentActions[' + studentId + '].exitRemarks=this.value">'
          + '</div>'
        );
    }
}

function loadSectionDropdown(studentId, classId, targetDivId, className) {
    $.get(URLS.getAvailSections, { classId: classId, targetSessionId: WZ.targetSessionId }, function (res) {
        var html = '<span class="small text-muted">' + className + ' — </span>';
        if (!res.data || !res.data.length) {
            html += '<span class="text-danger small">No sections in target</span>';
        } else {
            html += '<select class="form-select form-select-sm" style="display:inline-block;width:auto" onchange="WZ.studentActions[' + studentId + '].targetSectionId=parseInt(this.value)">';
            res.data.forEach(function (sec) {
                html += '<option value="' + sec.sectionId + '">' + sec.sectionName + '</option>';
            });
            html += '</select>';
            WZ.studentActions[studentId].targetSectionId = res.data[0].sectionId;
        }
        $('#' + targetDivId).html(html);
    });
}

function bulkAction(groupKey, action) {
    if (!action) return;
    WZ.allStudents.forEach(function (s) {
        var key = s.classId + '_' + s.sectionId;
        if (key === groupKey) {
            $('#row_' + s.studentId + ' .action-select').val(action);
            changeAction(s.studentId, action);
        }
    });
}

function step4Next() {
    // basic validation: leftschool must have exitReason
    var errors = [];
    WZ.allStudents.forEach(function (s) {
        var sa = WZ.studentActions[s.studentId];
        if (sa.action === 'leftschool' && !sa.exitReason) {
            errors.push(s.studentName + ' — Exit Reason required');
        }
        if (sa.action === 'retained' && !sa.retentionRemarks) {
            errors.push(s.studentName + ' — Retention remarks required');
        }
    });
    if (errors.length) {
        toastr.error(errors.slice(0, 3).join('<br>') + (errors.length > 3 ? '<br>...aur ' + (errors.length - 3) + ' aur' : ''), 'Validation Error', { escapeHtml: false });
        return;
    }
    renderPreview();
    goToStep(5);
}

// ── STEP 5: Preview ───────────────────────────────────────────────
function renderPreview() {
    var counts = { promote: 0, failed: 0, retained: 0, passout: 0, leftschool: 0 };
    WZ.allStudents.forEach(function (s) {
        var a = WZ.studentActions[s.studentId].action;
        if (counts[a] !== undefined) counts[a]++;
    });

    var srcName = $('#srcSession option:selected').text();
    var tgtName = $('#tgtSession option:selected').text();

    var stats = [
        { label: 'ClassSections to Copy',  count: WZ.selectedCSIds.length,   bg: '#dbeafe', color: '#1e40af' },
        { label: 'Timetable Entries',       count: WZ.selectedTTIds.length,   bg: '#ede9fe', color: '#5b21b6' },
        { label: 'Students Promote',        count: counts.promote,             bg: '#d1fae5', color: '#065f46' },
        { label: 'Students Failed',         count: counts.failed,              bg: '#fee2e2', color: '#991b1b' },
        { label: 'Students Retain (Other)', count: counts.retained,            bg: '#fef9c3', color: '#854d0e' },
        { label: 'Students Passout',        count: counts.passout,             bg: '#dbeafe', color: '#1e3a8a' },
        { label: 'Students Left School',    count: counts.leftschool,          bg: '#f1f5f9', color: '#334155' }
    ];

    var html = '<div class="alert alert-light border mb-3">'
             + '<strong>' + srcName + '</strong> &nbsp;→&nbsp; <strong>' + tgtName + '</strong>'
             + '</div>'
             + '<div class="row g-3 mb-4">';

    stats.forEach(function (s) {
        html += '<div class="col-6 col-md-3">'
              + '<div class="preview-stat" style="background:' + s.bg + '">'
              + '<div class="count" style="color:' + s.color + '">' + s.count + '</div>'
              + '<div class="label" style="color:' + s.color + '">' + s.label + '</div>'
              + '</div></div>';
    });
    html += '</div>';

    var total = Object.values(counts).reduce(function (a, b) { return a + b; }, 0)
              + WZ.selectedCSIds.length + WZ.selectedTTIds.length;

    if (total === 0) {
        html += '<div class="alert alert-warning">Koi changes configure nahi kiya. Confirm karne se kuch nahi hoga.</div>';
        $('#btnConfirm').prop('disabled', true);
    } else {
        $('#btnConfirm').prop('disabled', false);
    }

    $('#preview-body').html(html);
}

// ── Execute ───────────────────────────────────────────────────────
function executeRollover() {
    var payload = {
        sourceSessionId       : WZ.sourceSessionId,
        targetSessionId       : WZ.targetSessionId,
        selectedClassSectionIds: WZ.selectedCSIds,
        selectedTimetableIds  : WZ.selectedTTIds,
        studentActions        : []
    };

    WZ.allStudents.forEach(function (s) {
        var sa = WZ.studentActions[s.studentId];
        payload.studentActions.push({
            studentId       : s.studentId,
            action          : sa.action,
            targetClassId   : sa.targetClassId,
            targetSectionId : sa.targetSectionId,
            retentionRemarks: sa.retentionRemarks || null,
            exitReason      : sa.exitReason || null,
            exitDate        : sa.exitDate   || null,
            exitRemarks     : sa.exitRemarks|| null
        });
    });

    var modal = new bootstrap.Modal(document.getElementById('progressModal'));
    modal.show();
    setStage('cs', 'running');
    $('#exec-progress').css('width', '20%');
    $('#exec-message').text('Stage 1: ClassSection Rollover...');

    setTimeout(function () {
        setStage('cs', 'done');
        setStage('tt', 'running');
        $('#exec-progress').css('width', '50%');
        $('#exec-message').text('Stage 2: Timetable Rollover...');

        setTimeout(function () {
            setStage('tt', 'done');
            setStage('st', 'running');
            $('#exec-progress').css('width', '80%');
            $('#exec-message').text('Stage 3-7: Student Actions...');

            $.ajax({
                url         : URLS.execute,
                type        : 'POST',
                contentType : 'application/json',
                data        : JSON.stringify(payload),
                success     : function (res) {
                    $('#exec-progress').css('width', '100%');
                    if (res.success) {
                        setStage('st', 'done');
                        $('#exec-message').text('Promotion complete!');
                        setTimeout(function () {
                            modal.hide();
                            showSuccessSummary(res.summary);
                        }, 800);
                    } else {
                        setStage('st', 'error');
                        $('#exec-message').html('<span class="text-danger">' + res.message + '</span>');
                    }
                },
                error: function () {
                    setStage('st', 'error');
                    $('#exec-message').html('<span class="text-danger">Network error! Please try again.</span>');
                }
            });
        }, 600);
    }, 500);
}

function setStage(stage, state) {
    var icon = $('#si-' + stage);
    icon.attr('class', 'stage-icon ' + state);
    if (state === 'running') icon.html('<i class="fas fa-spinner fa-spin"></i>');
    else if (state === 'done') icon.html('<i class="fas fa-check"></i>');
    else if (state === 'error') icon.html('<i class="fas fa-times"></i>');
    else icon.html('<i class="fas fa-circle"></i>');
}

function showSuccessSummary(s) {
    Swal.fire({
        icon : 'success',
        title: 'Promotion Complete!',
        html : '<div class="text-start">'
             + '<b>ClassSections:</b> ' + s.classSectionCreated + ' created<br>'
             + '<b>Timetable:</b> '     + s.timetableCreated    + ' entries created<br>'
             + '<b>Promoted:</b> '      + s.promoted            + ' students<br>'
             + '<b>Failed:</b> '        + s.failed              + ' students<br>'
             + '<b>Retained (Other):</b> ' + s.retainedOther    + ' students<br>'
             + '<b>Passout:</b> '       + s.passout             + ' students<br>'
             + '<b>Left School:</b> '   + s.leftSchool          + ' students'
             + '</div>',
        confirmButtonText: 'View History'
    }).then(function () {
        window.location.href = '/SessionRollover/History';
    });
}

// ── Student History Modal ─────────────────────────────────────────
function showHistory(studentId) {
    $.get(URLS.getStudentHistory, { studentId: studentId }, function (res) {
        if (!res.success) return;
        var s = res.student;
        $('#historyModalTitle').text((s ? s.studentName : 'Student') + ' — Academic Journey');
        var actionColors = {
            Promoted: '#d1fae5', Failed: '#fee2e2', Retained: '#fef9c3',
            Passout: '#dbeafe', LeftSchool: '#f1f5f9', Manual: '#f8fafc'
        };
        var html = '<table class="table table-sm table-bordered" style="font-size:0.85rem">'
                 + '<thead class="table-dark"><tr><th>Session</th><th>Class</th><th>Section</th><th>Action</th><th>Notes</th></tr></thead><tbody>';
        res.sessions.forEach(function (row) {
            var bg = actionColors[row.promotionAction] || '#fff';
            html += '<tr style="background:' + bg + '">'
                  + '<td>' + row.sessionName + '</td>'
                  + '<td>' + row.className + '</td>'
                  + '<td>' + row.sectionName + '</td>'
                  + '<td><strong>' + row.promotionAction + '</strong></td>'
                  + '<td>' + (row.retentionRemarks || '') + '</td>'
                  + '</tr>';
        });
        if (!res.sessions.length) html += '<tr><td colspan="5" class="text-center text-muted">No enrollment records</td></tr>';
        html += '</tbody></table>';
        if (res.exits && res.exits.length) {
            html += '<h6 class="mt-3 text-danger">Exit Record</h6>'
                  + '<table class="table table-sm table-bordered" style="font-size:0.85rem">'
                  + '<thead class="table-danger"><tr><th>Session</th><th>Reason</th><th>Date</th><th>Remarks</th></tr></thead><tbody>';
            res.exits.forEach(function (e) {
                html += '<tr><td>' + e.sessionName + '</td><td>' + e.ExitReason + '</td><td>' + e.exitDate + '</td><td>' + (e.Remarks || '') + '</td></tr>';
            });
            html += '</tbody></table>';
        }
        $('#historyModalBody').html(html);
        new bootstrap.Modal(document.getElementById('historyModal')).show();
    });
}
