/**
 * School Document Builder — JavaScript Engine
 * Handles Drag & Drop, Property Editing, Templates, State History (Undo/Redo), and PDF/Print exports.
 *
 * ⚠ All element IDs, data attributes, and CSS classes in this file are aligned
 *   with Views/DocumentBuilder/Index.cshtml.  Do NOT rename IDs here without
 *   updating the View and vice-versa.
 */

class DocumentBuilder {
    constructor() {
        this.components = [];
        this.selectedComponentId = null;
        this.undoStack = [];
        this.redoStack = [];
        this.maxHistory = 50;
        this.autoSaveInterval = null;
        this.uploadingComponentId = null;
        this.copiedStyle = null;
        this.zoomLevel = 100;

        this.printSettings = {
            pageSize: 'A4',
            orientation: 'portrait',
            marginTop: 20,
            marginBottom: 20,
            marginLeft: 15,
            marginRight: 15,
            showHeader: false,
            headerText: '',
            showFooter: false,
            footerText: '',
            showPageNumbers: false,
            watermarkText: '',
            watermarkOpacity: 15
        };

        this.init();
    }

    init() {
        this.setupDragAndDrop();
        this.setupEventListeners();
        this.setupKeyboardShortcuts();
        this.loadInitialData();
        this.startAutoSave();
        this.updatePageNumbers();
    }

    // =========================================================================
    // INITIALIZATION & DATA LOADING
    // =========================================================================

    loadInitialData() {
        try {
            // Load Print Settings
            const printSettingsVal = document.getElementById('initialPrintSettingsJson')?.value;
            if (printSettingsVal && printSettingsVal !== '{}') {
                const parsed = JSON.parse(printSettingsVal);
                this.printSettings = { ...this.printSettings, ...parsed };
                this.applyPrintSettings();
            }

            // Load Components
            const componentsVal = document.getElementById('initialComponentsJson')?.value;
            if (componentsVal && componentsVal !== '[]') {
                const loadedComponents = JSON.parse(componentsVal);
                this.renderLoadedComponents(loadedComponents);
            }
        } catch (e) {
            console.error("Error loading initial document data:", e);
        }
    }

    renderLoadedComponents(loadedList) {
        const canvas = document.getElementById('docPaper');
        // Clear canvas but keep drop-placeholder element
        const placeholder = document.getElementById('dropPlaceholder');
        canvas.innerHTML = '';
        if (placeholder) canvas.appendChild(placeholder);

        // Sort by order
        loadedList.sort((a, b) => a.order - b.order);

        this.components = [];
        loadedList.forEach(comp => {
            const el = this.createComponentElement(comp.id, comp.type, comp.content, comp.style, comp);
            canvas.insertBefore(el, placeholder);
            this.components.push(comp);
        });

        if (this.components.length === 0) {
            this.showEmptyPlaceholder();
        } else {
            if (placeholder) placeholder.style.display = 'none';
            this.updateQuestionNumbers();
            this.updatePageNumbers();
        }

        this.saveState(); // First snapshot
    }

    // =========================================================================
    // DRAG & DROP ENGINE
    // =========================================================================

    setupDragAndDrop() {
        const docPaper = document.getElementById('docPaper');
        const cards = document.querySelectorAll('.component-card');

        cards.forEach(card => {
            card.addEventListener('dragstart', (e) => {
                e.dataTransfer.setData('text/plain', card.getAttribute('data-type'));
                card.classList.add('dragging');
            });

            card.addEventListener('dragend', () => {
                card.classList.remove('dragging');
            });
        });

        docPaper.addEventListener('dragover', (e) => {
            e.preventDefault();
            docPaper.classList.add('drag-over');
            this.showInsertIndicator(e);
        });

        docPaper.addEventListener('dragleave', () => {
            docPaper.classList.remove('drag-over');
            this.removeInsertIndicator();
        });

        docPaper.addEventListener('drop', (e) => {
            e.preventDefault();
            docPaper.classList.remove('drag-over');
            this.removeInsertIndicator();

            const type = e.dataTransfer.getData('text/plain');
            if (!type) return;

            const id = 'comp_' + Date.now();
            const defaultContent = this.getDefaultContentForType(type);
            const defaultStyle = this.getDefaultStyleForType(type);

            const newComponent = {
                id: id,
                type: type,
                content: defaultContent,
                style: defaultStyle,
                order: this.components.length,
                page: 1
            };

            // Add question fields if it's a question-type component
            const questionTypes = ['question', 'mcq', 'fillBlanks', 'trueFalse', 'matchFollowing'];
            if (questionTypes.includes(type)) {
                newComponent.questionNumber = this.getNextQuestionNumber();
                newComponent.marks = 2;
                newComponent.difficulty = 'Medium';
                newComponent.answerSpace = 3;
                if (type === 'mcq') {
                    newComponent.options = ['Option A', 'Option B', 'Option C', 'Option D'];
                }
            }

            // Determine insert index based on drop coordinates
            const dropTarget = this.getInsertIndex(e);

            const element = this.createComponentElement(id, type, defaultContent, defaultStyle, newComponent);

            if (dropTarget.element) {
                docPaper.insertBefore(element, dropTarget.element);
                this.components.splice(dropTarget.index, 0, newComponent);
            } else {
                // Insert before the drop placeholder
                const placeholder = document.getElementById('dropPlaceholder');
                if (placeholder) {
                    docPaper.insertBefore(element, placeholder);
                } else {
                    docPaper.appendChild(element);
                }
                this.components.push(newComponent);
            }

            // Hide placeholder
            const ph = document.getElementById('dropPlaceholder');
            if (ph) ph.style.display = 'none';

            // Recalculate orders
            this.recalculateOrders();
            this.selectComponent(id);
            this.updateQuestionNumbers();
            this.updatePageNumbers();

            this.saveState();
            this.showToaster('Component added!', 'success');
        });
    }

    showInsertIndicator(e) {
        this.removeInsertIndicator();
        const dropTarget = this.getInsertIndex(e);
        const indicator = document.createElement('div');
        indicator.id = 'insertIndicator';
        indicator.style.height = '4px';
        indicator.style.background = 'var(--db-accent, #3b82f6)';
        indicator.style.margin = '4px 0';
        indicator.style.borderRadius = '2px';
        indicator.style.transition = 'all 0.15s ease';

        const docPaper = document.getElementById('docPaper');
        if (dropTarget.element) {
            docPaper.insertBefore(indicator, dropTarget.element);
        } else {
            const placeholder = document.getElementById('dropPlaceholder');
            if (placeholder) {
                docPaper.insertBefore(indicator, placeholder);
            } else {
                docPaper.appendChild(indicator);
            }
        }
    }

    removeInsertIndicator() {
        document.getElementById('insertIndicator')?.remove();
    }

    getInsertIndex(e) {
        const docPaper = document.getElementById('docPaper');
        const children = Array.from(docPaper.querySelectorAll('.paper-component'));
        const clientY = e.clientY;

        for (let i = 0; i < children.length; i++) {
            const box = children[i].getBoundingClientRect();
            const middle = box.top + box.height / 2;
            if (clientY < middle) {
                return { element: children[i], index: i };
            }
        }
        return { element: null, index: children.length };
    }

    recalculateOrders() {
        const docPaper = document.getElementById('docPaper');
        const children = Array.from(docPaper.querySelectorAll('.paper-component'));

        children.forEach((el, index) => {
            const id = el.getAttribute('data-id');
            const comp = this.components.find(c => c.id === id);
            if (comp) {
                comp.order = index;
            }
        });
    }

    // =========================================================================
    // COMPONENTS FACTORY
    // Uses camelCase type names matching View's data-type attributes:
    //   schoolName, fillBlanks, trueFalse, matchFollowing, pageBreak
    // =========================================================================

    getDefaultContentForType(type) {
        switch (type) {
            case 'header':
                return '<h2>Annual Examination Notice / Header</h2>';
            case 'schoolName':
                return 'Krishak Inter College';
            case 'logo':
                return '';
            case 'title':
                return 'DOCUMENT TITLE';
            case 'paragraph':
                return 'This is a sample paragraph. Click to edit this text content directly on the page. Use the properties panel to adjust colors, fonts, spacing and alignment options.';
            case 'instructions':
                return '<strong>Instructions for Candidates:</strong><br/>1. Read the instructions carefully.<br/>2. Answer all questions.';
            case 'question':
                return 'Question text goes here. Answer all parts of this question.';
            case 'mcq':
                return 'Which of the following is correct?';
            case 'fillBlanks':
                return 'The capital of India is _______________ and the official currency is __________.';
            case 'trueFalse':
                return 'Photosynthesis takes place only in the presence of sunlight.';
            case 'matchFollowing':
                return 'Match the entries in Column A with Column B.';
            case 'table':
                return '<table class="table table-bordered mb-0"><thead><tr><th>Header 1</th><th>Header 2</th><th>Header 3</th></tr></thead><tbody><tr><td>Data 1</td><td>Data 2</td><td>Data 3</td></tr><tr><td>Data 4</td><td>Data 5</td><td>Data 6</td></tr></tbody></table>';
            case 'image':
                return '';
            case 'signature':
                return '<div class="d-flex justify-content-between"><div>___________________<br/><strong>Class Teacher</strong></div><div>___________________<br/><strong>Principal Signature</strong></div></div>';
            case 'footer':
                return 'Page 1 of 1 — Krishak Inter College, Ghatampur';
            case 'pageBreak':
                return '';
            default:
                return 'Sample Element';
        }
    }

    getDefaultStyleForType(type) {
        const base = {
            fontFamily: "'Inter', sans-serif",
            fontSize: '14px',
            color: '#1e293b',
            marginTop: '8px',
            marginBottom: '8px',
            paddingTop: '4px',
            paddingBottom: '4px',
            paddingLeft: '4px',
            paddingRight: '4px'
        };

        switch (type) {
            case 'schoolName':
                return { ...base, fontSize: '20px', fontWeight: 'bold', textAlign: 'center', color: '#1e3a8a' };
            case 'header':
                return { ...base, fontSize: '18px', fontWeight: 'bold', textAlign: 'center' };
            case 'title':
                return { ...base, fontSize: '16px', fontWeight: 'bold', textAlign: 'center', textDecoration: 'underline' };
            case 'instructions':
                return { ...base, fontSize: '12px', backgroundColor: '#f8fafc', borderLeft: '3px solid #cbd5e1', paddingTop: '8px', paddingBottom: '8px', paddingLeft: '12px' };
            case 'pageBreak':
                return { marginTop: '16px', marginBottom: '16px' };
            case 'logo':
                return { width: '80px', height: '80px', display: 'block', marginLeft: 'auto', marginRight: 'auto', marginTop: '8px', marginBottom: '8px' };
            case 'footer':
                return { ...base, fontSize: '11px', color: '#64748b', textAlign: 'center', borderTop: '1.5px solid #e2e8f0', paddingTop: '8px' };
            case 'signature':
                return { ...base, fontSize: '13px', marginTop: '40px' };
            default:
                return base;
        }
    }

    createComponentElement(id, type, content, style, compData) {
        const wrapper = document.createElement('div');
        wrapper.className = 'paper-component';
        wrapper.setAttribute('data-id', id);
        wrapper.setAttribute('data-type', type);

        // Styling
        this.applyStylesToElement(wrapper, style);

        // Component Inner Wrapper
        const inner = document.createElement('div');
        inner.className = 'comp-inner';

        const questionTypes = ['question', 'mcq', 'fillBlanks', 'trueFalse', 'matchFollowing'];

        if (type === 'logo') {
            inner.innerHTML = `<img src="${compData.src || '/kic_logo.png'}" style="width: 100%; height: 100%; object-fit: contain;" />`;
        } else if (type === 'image') {
            inner.innerHTML = compData.src
                ? `<img src="${compData.src}" style="max-width: 100%; height: auto;" />`
                : `<div class="image-placeholder py-4 text-center border text-muted" style="border-radius: 8px; border-style: dashed !important;">
                     <i class="bi bi-image" style="font-size: 2rem;"></i>
                     <p class="mb-0 small">Click to upload image</p>
                   </div>`;
        } else if (type === 'pageBreak') {
            inner.innerHTML = `<div class="page-break-line d-print-none"><i class="bi bi-scissors me-2"></i>Page Break (A4 Split Point)</div>`;
        } else if (type === 'mcq') {
            const questionText = `<div class="question-text" contenteditable="true">${content}</div>`;
            let optionsHtml = '<div class="mcq-options-grid mt-2">';
            const options = compData.options || ['Option A', 'Option B', 'Option C', 'Option D'];
            options.forEach((opt, index) => {
                optionsHtml += `<div class="mcq-option d-flex align-items-center gap-2">
                    <span class="mcq-bullet fw-medium" style="width: 20px;">(${String.fromCharCode(65 + index)})</span>
                    <span class="mcq-text flex-grow-1" contenteditable="true">${opt}</span>
                </div>`;
            });
            optionsHtml += '</div>';
            inner.innerHTML = `<div class="q-header d-flex justify-content-between mb-1">
                <span class="badge bg-secondary-subtle text-secondary me-2 q-num-badge">Q. ${compData.questionNumber || ''}</span>
                <span class="small text-muted fw-semibold">(${compData.marks || 1} Marks)</span>
            </div>
            ${questionText}
            ${optionsHtml}`;
        } else if (type === 'question' || type === 'fillBlanks' || type === 'trueFalse' || type === 'matchFollowing') {
            const ansLines = (compData.answerSpace || 3) * 24;
            inner.innerHTML = `<div class="q-header d-flex justify-content-between mb-1">
                <span class="badge bg-secondary-subtle text-secondary me-2 q-num-badge">Q. ${compData.questionNumber || ''}</span>
                <span class="small text-muted fw-semibold">(${compData.marks || 1} Marks)</span>
            </div>
            <div class="question-text" contenteditable="true">${content}</div>
            <div class="answer-space mt-2" style="border-bottom: 1.5px dashed #cbd5e1; height: ${ansLines}px;"></div>`;
        } else {
            inner.innerHTML = content;
            inner.contentEditable = 'true';
        }

        wrapper.appendChild(inner);

        // Floating Action controls on hover
        const actions = document.createElement('div');
        actions.className = 'component-actions d-print-none';
        actions.innerHTML = `
            <button class="action-btn drag-handle" title="Drag to reposition"><i class="bi bi-arrows-move"></i></button>
            <button class="action-btn duplicate-btn" title="Duplicate component"><i class="bi bi-copy"></i></button>
            <button class="action-btn delete-btn" title="Delete component"><i class="bi bi-trash"></i></button>
        `;
        wrapper.appendChild(actions);

        // Bind element events
        this.bindComponentEvents(wrapper);

        return wrapper;
    }

    applyStylesToElement(el, styleObj) {
        if (!styleObj) return;
        Object.keys(styleObj).forEach(key => {
            try {
                el.style[key] = styleObj[key];
            } catch (e) { /* skip invalid */ }
        });
    }

    bindComponentEvents(el) {
        const id = el.getAttribute('data-id');
        const type = el.getAttribute('data-type');
        const questionTypes = ['question', 'mcq', 'fillBlanks', 'trueFalse', 'matchFollowing'];

        // Selection on click
        el.addEventListener('click', (e) => {
            e.stopPropagation();
            this.selectComponent(id);
        });

        // Right-click context menu
        el.addEventListener('contextmenu', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.selectComponent(id);
            this.showContextMenu(e.pageX, e.pageY);
        });

        // Content updates for simple editable components
        const inner = el.querySelector('.comp-inner');
        if (inner && inner.contentEditable === 'true') {
            inner.addEventListener('blur', () => {
                const comp = this.components.find(c => c.id === id);
                if (comp) {
                    comp.content = inner.innerHTML;
                    this.saveState();
                }
            });
        }

        // MCQ text bindings
        if (type === 'mcq') {
            const qText = inner.querySelector('.question-text');
            qText?.addEventListener('blur', () => {
                const comp = this.components.find(c => c.id === id);
                if (comp) {
                    comp.content = qText.innerHTML;
                    this.saveState();
                }
            });

            const optElements = inner.querySelectorAll('.mcq-text');
            optElements.forEach((optEl, idx) => {
                optEl.addEventListener('blur', () => {
                    const comp = this.components.find(c => c.id === id);
                    if (comp && comp.options) {
                        comp.options[idx] = optEl.innerText;
                        this.saveState();
                    }
                });
            });
        }

        // Question text binding
        if (questionTypes.includes(type) && type !== 'mcq') {
            const qText = inner.querySelector('.question-text');
            qText?.addEventListener('blur', () => {
                const comp = this.components.find(c => c.id === id);
                if (comp) {
                    comp.content = qText.innerHTML;
                    this.saveState();
                }
            });
        }

        // Image upload trigger
        if (type === 'image' || type === 'logo') {
            inner.addEventListener('click', (e) => {
                e.stopPropagation();
                this.selectComponent(id);
                this.openImageUploadModal(id);
            });
        }

        // Duplicate button
        el.querySelector('.duplicate-btn')?.addEventListener('click', (e) => {
            e.stopPropagation();
            this.duplicateComponent(id);
        });

        // Delete button
        el.querySelector('.delete-btn')?.addEventListener('click', (e) => {
            e.stopPropagation();
            this.deleteComponent(id);
        });
    }

    // =========================================================================
    // CONTEXT MENU  (right-click on components)
    // =========================================================================

    showContextMenu(x, y) {
        const menu = document.getElementById('builderContextMenu');
        if (!menu) return;
        menu.style.display = 'block';
        menu.style.left = x + 'px';
        menu.style.top = y + 'px';

        // Close on next outside click
        const closeHandler = () => {
            menu.style.display = 'none';
            document.removeEventListener('click', closeHandler);
        };
        setTimeout(() => document.addEventListener('click', closeHandler), 10);
    }

    contextAction(action) {
        const menu = document.getElementById('builderContextMenu');
        if (menu) menu.style.display = 'none';

        if (!this.selectedComponentId) return;

        switch (action) {
            case 'duplicate':
                this.duplicateComponent(this.selectedComponentId);
                break;
            case 'moveUp':
                this.moveComponent(this.selectedComponentId, -1);
                break;
            case 'moveDown':
                this.moveComponent(this.selectedComponentId, 1);
                break;
            case 'copyStyle':
                this.copyComponentStyle();
                break;
            case 'pasteStyle':
                this.pasteComponentStyle();
                break;
            case 'delete':
                this.deleteComponent(this.selectedComponentId);
                break;
        }
    }

    moveComponent(id, direction) {
        const idx = this.components.findIndex(c => c.id === id);
        if (idx === -1) return;

        const newIdx = idx + direction;
        if (newIdx < 0 || newIdx >= this.components.length) return;

        // Swap in data
        [this.components[idx], this.components[newIdx]] = [this.components[newIdx], this.components[idx]];

        // Swap in DOM
        const docPaper = document.getElementById('docPaper');
        const children = Array.from(docPaper.querySelectorAll('.paper-component'));
        const el = children[idx];
        const target = children[newIdx];

        if (direction === -1) {
            docPaper.insertBefore(el, target);
        } else {
            docPaper.insertBefore(target, el);
        }

        this.recalculateOrders();
        this.updateQuestionNumbers();
        this.saveState();
    }

    copyComponentStyle() {
        const comp = this.components.find(c => c.id === this.selectedComponentId);
        if (comp) {
            this.copiedStyle = JSON.parse(JSON.stringify(comp.style || {}));
            this.showToaster('Style copied!', 'success');
        }
    }

    pasteComponentStyle() {
        if (!this.copiedStyle) {
            this.showToaster('No style copied yet.', 'warning');
            return;
        }
        const comp = this.components.find(c => c.id === this.selectedComponentId);
        const el = document.querySelector(`.paper-component[data-id="${this.selectedComponentId}"]`);
        if (comp && el) {
            comp.style = JSON.parse(JSON.stringify(this.copiedStyle));
            this.applyStylesToElement(el, comp.style);
            this.renderPropertiesPanel(comp, el);
            this.saveState();
            this.showToaster('Style pasted!', 'success');
        }
    }

    // =========================================================================
    // SELECTION & PROPERTIES PANEL BINDING
    //
    // View IDs used:
    //   #propsPlaceholder    — "Select a component..." placeholder
    //   #propsContent        — the actual properties form container
    //   #questionPropsGroup  — question-specific property group
    //   #mcqOptionsSection   — MCQ options section inside question props
    //   #propComponentType   — component type label
    //   All property inputs use [data-property="xxx"] attributes
    // =========================================================================

    selectComponent(id) {
        // Deselect previous
        document.querySelectorAll('.paper-component.selected-component').forEach(el => {
            el.classList.remove('selected-component');
        });

        this.selectedComponentId = id;
        const el = document.querySelector(`.paper-component[data-id="${id}"]`);
        if (!el) return;

        el.classList.add('selected-component');

        // Show properties
        const comp = this.components.find(c => c.id === id);
        if (comp) {
            this.renderPropertiesPanel(comp, el);
        }
    }

    renderPropertiesPanel(comp, el) {
        // Toggle placeholder vs content
        const placeholder = document.getElementById('propsPlaceholder');
        const content = document.getElementById('propsContent');
        const questionProps = document.getElementById('questionPropsGroup');

        if (placeholder) placeholder.style.display = 'none';
        if (content) content.style.display = 'block';

        // Set component type label
        const typeLabel = document.getElementById('propComponentType');
        if (typeLabel) {
            typeLabel.textContent = this.getTypeLabel(comp.type);
        }

        // Read styles from component data
        const style = comp.style || {};

        // Set all property inputs using data-property attribute selectors
        this.setPropValue('fontFamily', style.fontFamily || "'Inter', sans-serif");
        this.setPropValue('fontSize', parseInt(style.fontSize) || 14);
        this.setPropValue('fontWeight', style.fontWeight || 'normal');
        this.setPropValue('fontStyle', style.fontStyle || 'normal');
        this.setPropValue('textDecoration', style.textDecoration || 'none');
        this.setPropValue('lineHeight', parseFloat(style.lineHeight) || 1.5);
        this.setPropValue('letterSpacing', parseInt(style.letterSpacing) || 0);

        // Colors
        this.setPropValue('color', this.toHex(style.color) || '#000000');
        this.setPropValue('backgroundColor', this.toHex(style.backgroundColor) || '#ffffff');
        this.setPropValue('outlineColor', this.toHex(style.outlineColor) || '#ffffff');

        // Update color hex labels
        const colorHex = document.getElementById('colorHex');
        const bgColorHex = document.getElementById('bgColorHex');
        const highlightHex = document.getElementById('highlightHex');
        if (colorHex) colorHex.textContent = this.toHex(style.color) || '#000000';
        if (bgColorHex) bgColorHex.textContent = this.toHex(style.backgroundColor) || '#ffffff';
        if (highlightHex) highlightHex.textContent = this.toHex(style.outlineColor) || '#ffffff';

        // Alignment buttons — View uses .prop-btn with data-value
        document.querySelectorAll('#textAlignGroup .prop-btn').forEach(btn => {
            btn.classList.remove('active');
            if (btn.getAttribute('data-value') === (style.textAlign || 'left')) {
                btn.classList.add('active');
            }
        });

        // Spacing — Margin
        this.setPropValue('marginTop', parseInt(style.marginTop) || 0);
        this.setPropValue('marginRight', parseInt(style.marginRight) || 0);
        this.setPropValue('marginBottom', parseInt(style.marginBottom) || 0);
        this.setPropValue('marginLeft', parseInt(style.marginLeft) || 0);

        // Spacing — Padding
        this.setPropValue('paddingTop', parseInt(style.paddingTop) || 0);
        this.setPropValue('paddingRight', parseInt(style.paddingRight) || 0);
        this.setPropValue('paddingBottom', parseInt(style.paddingBottom) || 0);
        this.setPropValue('paddingLeft', parseInt(style.paddingLeft) || 0);

        // Dimensions
        this.setPropValue('width', style.width || '');
        this.setPropValue('height', style.height || '');
        this.setPropValue('maxWidth', style.maxWidth || '');

        // Border
        this.setPropValue('borderWidth', parseInt(style.borderWidth) || 0);
        this.setPropValue('borderStyle', style.borderStyle || 'none');
        this.setPropValue('borderColor', this.toHex(style.borderColor) || '#cccccc');
        this.setPropValue('borderRadius', parseInt(style.borderRadius) || 0);

        const borderColorHex = document.getElementById('borderColorHex');
        if (borderColorHex) borderColorHex.textContent = this.toHex(style.borderColor) || '#cccccc';

        // Display/Visibility
        this.setPropValue('display', style.display || 'block');
        this.setPropValue('visibility', style.visibility || 'visible');

        // Question properties
        const questionTypes = ['question', 'mcq', 'fillBlanks', 'trueFalse', 'matchFollowing'];
        if (questionTypes.includes(comp.type)) {
            if (questionProps) questionProps.style.display = 'block';

            // View uses #propQuestionType, #propMarks, #propDifficulty, #propAnswerLines
            const qTypeSelect = document.getElementById('propQuestionType');
            if (qTypeSelect) qTypeSelect.value = comp.type === 'mcq' ? 'mcq' : (comp.type === 'fillBlanks' ? 'fillBlanks' : (comp.type === 'trueFalse' ? 'trueFalse' : (comp.type === 'matchFollowing' ? 'matchFollowing' : 'short')));

            const marksInput = document.getElementById('propMarks');
            if (marksInput) marksInput.value = comp.marks || 1;

            const diffSelect = document.getElementById('propDifficulty');
            if (diffSelect) diffSelect.value = comp.difficulty || 'Medium';

            const ansLinesInput = document.getElementById('propAnswerLines');
            if (ansLinesInput) ansLinesInput.value = comp.answerSpace || 3;

            // MCQ Options section visibility
            const mcqSection = document.getElementById('mcqOptionsSection');
            if (comp.type === 'mcq' && mcqSection) {
                mcqSection.style.display = 'block';
                this.renderMcqOptionInputs(comp);
            } else if (mcqSection) {
                mcqSection.style.display = 'none';
            }
        } else {
            if (questionProps) questionProps.style.display = 'none';
        }
    }

    /**
     * Sets the value of a property input using the [data-property] attribute selector.
     */
    setPropValue(propName, value) {
        const input = document.querySelector(`[data-property="${propName}"]`);
        if (input) {
            input.value = value;
        }
    }

    /**
     * Converts a CSS color value to hex format for color inputs.
     */
    toHex(color) {
        if (!color) return null;
        if (color.startsWith('#')) return color;
        if (color.startsWith('rgb')) {
            const match = color.match(/\d+/g);
            if (match && match.length >= 3) {
                return '#' + match.slice(0, 3).map(n => parseInt(n).toString(16).padStart(2, '0')).join('');
            }
        }
        // Named colors — return as-is and let browser handle it
        return color;
    }

    getTypeLabel(type) {
        const labels = {
            'header': 'Header',
            'footer': 'Footer',
            'pageBreak': 'Page Break',
            'schoolName': 'School Name',
            'logo': 'Logo',
            'title': 'Title',
            'paragraph': 'Paragraph',
            'instructions': 'Instructions',
            'image': 'Image',
            'question': 'Question',
            'mcq': 'MCQ',
            'fillBlanks': 'Fill in Blanks',
            'trueFalse': 'True / False',
            'matchFollowing': 'Match Following',
            'table': 'Table',
            'signature': 'Signature'
        };
        return labels[type] || type;
    }

    /**
     * Called from View's oninput/onchange handlers via the global bridge.
     * Updates a CSS style property on the currently selected component.
     */
    updateComponentProperty(propName, value) {
        if (!this.selectedComponentId) return;

        const comp = this.components.find(c => c.id === this.selectedComponentId);
        const el = document.querySelector(`.paper-component[data-id="${this.selectedComponentId}"]`);

        if (!comp || !el) return;

        if (!comp.style) comp.style = {};

        // Store and apply
        comp.style[propName] = value;
        el.style[propName] = value;

        this.saveState();
    }

    /**
     * Called from View for question-specific data properties (marks, difficulty, answerLines, questionType).
     */
    updateComponentData(dataKey, value) {
        if (!this.selectedComponentId) return;

        const comp = this.components.find(c => c.id === this.selectedComponentId);
        if (!comp) return;

        if (dataKey === 'marks') {
            comp.marks = parseFloat(value) || 1;
            // Update marks badge on canvas
            const el = document.querySelector(`.paper-component[data-id="${comp.id}"]`);
            const marksBadge = el?.querySelector('.q-header .small.text-muted');
            if (marksBadge) marksBadge.textContent = `(${comp.marks} Marks)`;
        } else if (dataKey === 'difficulty') {
            comp.difficulty = value;
        } else if (dataKey === 'answerLines') {
            comp.answerSpace = parseInt(value) || 3;
            const el = document.querySelector(`.paper-component[data-id="${comp.id}"]`);
            const ansSpace = el?.querySelector('.answer-space');
            if (ansSpace) ansSpace.style.height = (comp.answerSpace * 24) + 'px';
        } else if (dataKey === 'questionType') {
            // Changing question type is complex — for now just store it
            comp.questionSubType = value;
        }

        this.saveState();
    }

    // =========================================================================
    // MCQ OPTION MANAGEMENT
    // View uses #mcqOptionsList for the container (inside #mcqOptionsSection)
    // =========================================================================

    renderMcqOptionInputs(comp) {
        const container = document.getElementById('mcqOptionsList');
        if (!container) return;

        container.innerHTML = '';
        const options = comp.options || ['Option A', 'Option B', 'Option C', 'Option D'];

        options.forEach((opt, idx) => {
            const row = document.createElement('div');
            row.className = 'mcq-option-row';
            row.setAttribute('data-index', idx);
            row.innerHTML = `
                <span class="mcq-option-letter">${String.fromCharCode(65 + idx)}</span>
                <input type="text" class="prop-input mcq-option-input" placeholder="Option ${String.fromCharCode(65 + idx)}" value="${this.escapeHtml(opt)}" />
                <button type="button" class="mcq-option-remove" onclick="builder.removeMcqOption(${idx})" title="Remove">
                    <i class="bi bi-x"></i>
                </button>
            `;
            container.appendChild(row);
        });

        // Bind events to inputs
        container.querySelectorAll('.mcq-option-input').forEach((input, idx) => {
            input.addEventListener('input', () => {
                comp.options[idx] = input.value;
                // Sync preview on canvas
                const el = document.querySelector(`.paper-component[data-id="${comp.id}"]`);
                const optionLabel = el?.querySelectorAll('.mcq-text')[idx];
                if (optionLabel) optionLabel.innerText = input.value;
            });
        });
    }

    addMcqOption() {
        const comp = this.components.find(c => c.id === this.selectedComponentId);
        if (!comp || comp.type !== 'mcq') return;

        comp.options = comp.options || [];
        if (comp.options.length >= 8) {
            this.showToaster('Maximum 8 options allowed.', 'warning');
            return;
        }
        const nextChar = String.fromCharCode(65 + comp.options.length);
        comp.options.push('Option ' + nextChar);

        this.rebuildComponentElement(comp);
        this.renderMcqOptionInputs(comp);
        this.saveState();
    }

    removeMcqOption(idx) {
        const comp = this.components.find(c => c.id === this.selectedComponentId);
        if (!comp || comp.type !== 'mcq') return;

        comp.options = comp.options || [];
        if (comp.options.length <= 2) {
            this.showToaster('MCQ must have at least 2 options.', 'warning');
            return;
        }

        comp.options.splice(idx, 1);
        this.rebuildComponentElement(comp);
        this.renderMcqOptionInputs(comp);
        this.saveState();
    }

    /**
     * Called from the View's MCQ option inputs via oninput.
     */
    updateMcqOption(idx, value) {
        const comp = this.components.find(c => c.id === this.selectedComponentId);
        if (!comp || !comp.options) return;

        comp.options[idx] = value;

        // Sync preview
        const el = document.querySelector(`.paper-component[data-id="${comp.id}"]`);
        const optionLabel = el?.querySelectorAll('.mcq-text')[idx];
        if (optionLabel) optionLabel.innerText = value;
    }

    rebuildComponentElement(comp) {
        const oldEl = document.querySelector(`.paper-component[data-id="${comp.id}"]`);
        if (!oldEl) return;

        const newEl = this.createComponentElement(comp.id, comp.type, comp.content, comp.style, comp);
        oldEl.parentNode.replaceChild(newEl, oldEl);
        this.selectComponent(comp.id);
    }

    getNextQuestionNumber() {
        const questionTypes = ['question', 'mcq', 'fillBlanks', 'trueFalse', 'matchFollowing'];
        const questions = this.components.filter(c => questionTypes.includes(c.type));
        return questions.length + 1;
    }

    updateQuestionNumbers() {
        let qNum = 1;
        const questionTypes = ['question', 'mcq', 'fillBlanks', 'trueFalse', 'matchFollowing'];
        this.components.forEach(comp => {
            if (questionTypes.includes(comp.type)) {
                comp.questionNumber = qNum;
                const el = document.querySelector(`.paper-component[data-id="${comp.id}"]`);
                const badge = el?.querySelector('.q-num-badge');
                if (badge) badge.innerText = `Q. ${qNum}`;
                qNum++;
            }
        });
    }

    escapeHtml(str) {
        const div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    }

    // =========================================================================
    // PAGE MANAGEMENT
    // =========================================================================

    updatePageNumbers() {
        const breakComponents = this.components.filter(c => c.type === 'pageBreak');
        const totalPages = breakComponents.length + 1;

        // Update footer text with page numbers if enabled
        if (this.printSettings.showPageNumbers) {
            const footerComps = this.components.filter(c => c.type === 'footer');
            footerComps.forEach(fc => {
                const el = document.querySelector(`.paper-component[data-type="footer"][data-id="${fc.id}"] .comp-inner`);
                // Don't auto-overwrite user content
            });
        }
    }

    // =========================================================================
    // EVENT BINDINGS & ACTIONS
    // =========================================================================

    setupEventListeners() {
        // Document Name binding
        const nameInput = document.getElementById('documentNameInput');
        if (nameInput) {
            nameInput.addEventListener('blur', () => this.saveState());
        }

        // Click outside components to deselect
        const paperWrapper = document.querySelector('.doc-paper-wrapper');
        if (paperWrapper) {
            paperWrapper.addEventListener('click', (e) => {
                // Only deselect if clicking directly on wrapper/paper background, not on a component
                if (e.target === paperWrapper || e.target.id === 'docPaper' || e.target.id === 'dropPlaceholder' || e.target.closest('#dropPlaceholder')) {
                    this.deselectAll();
                }
            });
        }

        // Close context menu on document click
        document.addEventListener('click', () => {
            const menu = document.getElementById('builderContextMenu');
            if (menu) menu.style.display = 'none';
        });
    }

    deselectAll() {
        document.querySelectorAll('.paper-component.selected-component').forEach(el => {
            el.classList.remove('selected-component');
        });
        this.selectedComponentId = null;

        // Show placeholder, hide form — using correct View IDs
        const placeholder = document.getElementById('propsPlaceholder');
        const content = document.getElementById('propsContent');
        const questionProps = document.getElementById('questionPropsGroup');

        if (placeholder) placeholder.style.display = 'flex';
        if (content) content.style.display = 'none';
        if (questionProps) questionProps.style.display = 'none';
    }

    duplicateComponent(id) {
        const comp = this.components.find(c => c.id === id);
        if (!comp) return;

        const newId = 'comp_' + Date.now();
        const duplicatedComp = JSON.parse(JSON.stringify(comp));
        duplicatedComp.id = newId;
        duplicatedComp.order = comp.order + 1;

        const questionTypes = ['question', 'mcq', 'fillBlanks', 'trueFalse', 'matchFollowing'];
        if (questionTypes.includes(duplicatedComp.type)) {
            duplicatedComp.questionNumber = this.getNextQuestionNumber();
        }

        const el = document.querySelector(`.paper-component[data-id="${id}"]`);
        const newEl = this.createComponentElement(newId, comp.type, comp.content, comp.style, duplicatedComp);

        el.parentNode.insertBefore(newEl, el.nextSibling);
        this.components.splice(comp.order + 1, 0, duplicatedComp);

        this.recalculateOrders();
        this.selectComponent(newId);
        this.updateQuestionNumbers();
        this.updatePageNumbers();
        this.saveState();
        this.showToaster('Component duplicated!', 'success');
    }

    deleteComponent(id) {
        const idx = this.components.findIndex(c => c.id === id);
        if (idx === -1) return;

        const el = document.querySelector(`.paper-component[data-id="${id}"]`);
        if (el) {
            el.classList.add('fade-out');
            setTimeout(() => {
                el.remove();
                this.components.splice(idx, 1);
                this.recalculateOrders();

                if (this.selectedComponentId === id) {
                    this.deselectAll();
                }

                if (this.components.length === 0) {
                    this.showEmptyPlaceholder();
                } else {
                    this.updateQuestionNumbers();
                    this.updatePageNumbers();
                }

                this.saveState();
                this.showToaster('Component deleted.', 'success');
            }, 200);
        }
    }

    deleteSelectedComponent() {
        if (this.selectedComponentId) {
            this.deleteComponent(this.selectedComponentId);
        }
    }

    showEmptyPlaceholder() {
        const ph = document.getElementById('dropPlaceholder');
        if (ph) ph.style.display = 'flex';
    }

    // =========================================================================
    // STATE ENGINE (UNDO / REDO)
    // =========================================================================

    saveState() {
        const state = {
            components: JSON.parse(JSON.stringify(this.components)),
            printSettings: JSON.parse(JSON.stringify(this.printSettings))
        };

        this.undoStack.push(state);
        if (this.undoStack.length > this.maxHistory) {
            this.undoStack.shift();
        }

        this.redoStack = [];
    }

    undo() {
        if (this.undoStack.length <= 1) return;

        const currentState = this.undoStack.pop();
        this.redoStack.push(currentState);

        const prevState = this.undoStack[this.undoStack.length - 1];
        this.applyState(prevState);
        this.showToaster('Undo', 'info');
    }

    redo() {
        if (this.redoStack.length === 0) return;

        const state = this.redoStack.pop();
        this.undoStack.push(state);

        this.applyState(state);
        this.showToaster('Redo', 'info');
    }

    applyState(state) {
        this.components = JSON.parse(JSON.stringify(state.components));
        this.printSettings = JSON.parse(JSON.stringify(state.printSettings));

        this.applyPrintSettings();

        // Re-render A4 preview
        const canvas = document.getElementById('docPaper');
        const placeholder = document.getElementById('dropPlaceholder');
        canvas.innerHTML = '';
        if (placeholder) canvas.appendChild(placeholder);

        if (this.components.length === 0) {
            this.showEmptyPlaceholder();
        } else {
            if (placeholder) placeholder.style.display = 'none';
            this.components.forEach(comp => {
                const el = this.createComponentElement(comp.id, comp.type, comp.content, comp.style, comp);
                canvas.insertBefore(el, placeholder);
            });
            this.updateQuestionNumbers();
            this.updatePageNumbers();
        }

        if (this.selectedComponentId) {
            this.selectComponent(this.selectedComponentId);
        }
    }

    setupKeyboardShortcuts() {
        window.addEventListener('keydown', (e) => {
            if (e.ctrlKey && e.key === 'z') {
                e.preventDefault();
                this.undo();
            }
            if (e.ctrlKey && e.key === 'y') {
                e.preventDefault();
                this.redo();
            }
            if (e.ctrlKey && e.key === 's') {
                e.preventDefault();
                this.saveDocument('Draft');
            }
            if (e.key === 'Delete' && this.selectedComponentId) {
                if (document.activeElement.contentEditable === 'true' || document.activeElement.tagName === 'INPUT' || document.activeElement.tagName === 'TEXTAREA' || document.activeElement.tagName === 'SELECT') {
                    return;
                }
                e.preventDefault();
                this.deleteComponent(this.selectedComponentId);
            }
        });
    }

    // =========================================================================
    // PRINT SETTINGS & MARGINS
    //
    // View element IDs (Index.cshtml):
    //   #settingPageSize, input[name="settingOrientation"],
    //   #settingMarginTop/Right/Bottom/Left,
    //   #settingHeaderToggle, #settingHeaderText,
    //   #settingFooterToggle, #settingFooterText,
    //   #settingPageNumbers, #settingWatermarkText, #settingWatermarkOpacity
    // =========================================================================

    applyPrintSettings() {
        const paper = document.getElementById('docPaper');
        if (!paper) return;

        // Apply margins in mm
        paper.style.paddingTop = this.printSettings.marginTop + 'mm';
        paper.style.paddingBottom = this.printSettings.marginBottom + 'mm';
        paper.style.paddingLeft = this.printSettings.marginLeft + 'mm';
        paper.style.paddingRight = this.printSettings.marginRight + 'mm';

        // Orientation
        if (this.printSettings.orientation === 'landscape') {
            paper.classList.add('landscape');
        } else {
            paper.classList.remove('landscape');
        }

        // Watermark
        let watermark = document.getElementById('paperWatermark');
        if (this.printSettings.watermarkText) {
            if (!watermark) {
                watermark = document.createElement('div');
                watermark.id = 'paperWatermark';
                watermark.className = 'paper-watermark';
                paper.appendChild(watermark);
            }
            watermark.innerText = this.printSettings.watermarkText;
            watermark.style.opacity = (this.printSettings.watermarkOpacity / 100).toString();
            watermark.style.display = 'flex';
        } else if (watermark) {
            watermark.style.display = 'none';
        }
    }

    /**
     * Called by the "Apply Settings" button in the Print Settings Modal.
     * Reads values from the modal form and applies them.
     */
    applyPrintSettingsFromModal() {
        this.printSettings.pageSize = document.getElementById('settingPageSize')?.value || 'A4';
        this.printSettings.orientation = document.querySelector('input[name="settingOrientation"]:checked')?.value || 'portrait';

        this.printSettings.marginTop = parseFloat(document.getElementById('settingMarginTop')?.value) || 20;
        this.printSettings.marginRight = parseFloat(document.getElementById('settingMarginRight')?.value) || 15;
        this.printSettings.marginBottom = parseFloat(document.getElementById('settingMarginBottom')?.value) || 20;
        this.printSettings.marginLeft = parseFloat(document.getElementById('settingMarginLeft')?.value) || 15;

        this.printSettings.showHeader = document.getElementById('settingHeaderToggle')?.checked || false;
        this.printSettings.headerText = document.getElementById('settingHeaderText')?.value || '';
        this.printSettings.showFooter = document.getElementById('settingFooterToggle')?.checked || false;
        this.printSettings.footerText = document.getElementById('settingFooterText')?.value || '';

        this.printSettings.showPageNumbers = document.getElementById('settingPageNumbers')?.checked || false;
        this.printSettings.watermarkText = document.getElementById('settingWatermarkText')?.value || '';
        this.printSettings.watermarkOpacity = parseFloat(document.getElementById('settingWatermarkOpacity')?.value) || 15;

        this.applyPrintSettings();
        this.saveState();

        // Close modal
        const modalEl = document.getElementById('printSettingsModal');
        const modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();

        this.showToaster('Print settings applied!', 'success');
    }

    // =========================================================================
    // TEMPLATE LOADING
    // =========================================================================

    loadTemplate(templateId) {
        fetch('/DocumentBuilder/GetTemplate?id=' + templateId)
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    try {
                        this.printSettings = { ...this.printSettings, ...JSON.parse(data.printSettingsJson || '{}') };
                    } catch (e) { /* keep defaults */ }
                    this.applyPrintSettings();

                    const newComponents = JSON.parse(data.componentsJson || '[]');
                    this.components = [];
                    this.renderLoadedComponents(newComponents);

                    // Set document type to template type
                    const docTypeSelect = document.getElementById('documentTypeSelect');
                    if (docTypeSelect && data.templateType) {
                        docTypeSelect.value = data.templateType;
                    }

                    // Close Modal
                    const modalEl = document.getElementById('templateModal');
                    const modal = bootstrap.Modal.getInstance(modalEl);
                    if (modal) modal.hide();

                    this.showToaster('Template loaded!', 'success');
                } else {
                    this.showToaster('Error: ' + data.message, 'danger');
                }
            })
            .catch(err => {
                console.error(err);
                this.showToaster('Connection error loading template.', 'danger');
            });
    }

    filterTemplates(filterType, btn) {
        // Update active filter button
        document.querySelectorAll('.template-filter-btn').forEach(b => b.classList.remove('active'));
        if (btn) btn.classList.add('active');

        // Show/hide template cards
        document.querySelectorAll('.template-card').forEach(card => {
            const cardType = card.getAttribute('data-template-type');
            if (filterType === 'all' || cardType === filterType) {
                card.style.display = '';
            } else {
                card.style.display = 'none';
            }
        });
    }

    // =========================================================================
    // IMAGE UPLOAD SYSTEM
    //
    // View element IDs:
    //   #imageUploadModal, #imageDropZone, #imageFileInput,
    //   #imagePreviewArea, #imagePreview, #btnUploadImage
    // =========================================================================

    openImageUploadModal(componentId) {
        this.uploadingComponentId = componentId;
        clearImagePreview(); // global helper in Index.cshtml
        const modal = new bootstrap.Modal(document.getElementById('imageUploadModal'));
        modal.show();
    }

    uploadImage() {
        const fileInput = document.getElementById('imageFileInput');
        const file = fileInput?.files[0];
        if (!file) {
            this.showToaster('Please select a file to upload.', 'warning');
            return;
        }

        const btn = document.getElementById('btnUploadImage');
        if (btn) {
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Uploading...';
        }

        const formData = new FormData();
        formData.append('file', file);

        const docId = document.getElementById('currentDocumentId')?.value;
        if (docId) formData.append('documentId', docId);

        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');

        fetch('/DocumentBuilder/UploadImage', {
            method: 'POST',
            body: formData,
            headers: {
                'RequestVerificationToken': tokenInput?.value || ''
            }
        })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                const comp = this.components.find(c => c.id === this.uploadingComponentId);
                if (comp) {
                    comp.src = data.filePath;

                    const el = document.querySelector(`.paper-component[data-id="${comp.id}"]`);
                    const inner = el?.querySelector('.comp-inner');
                    if (inner) {
                        if (comp.type === 'logo') {
                            inner.innerHTML = `<img src="${data.filePath}" style="width: 100%; height: 100%; object-fit: contain;" />`;
                        } else {
                            inner.innerHTML = `<img src="${data.filePath}" style="max-width: 100%; height: auto;" />`;
                        }
                    }
                    this.saveState();
                }

                // Close Modal
                const modalEl = document.getElementById('imageUploadModal');
                const modal = bootstrap.Modal.getInstance(modalEl);
                if (modal) modal.hide();

                this.showToaster('Image uploaded!', 'success');
            } else {
                this.showToaster('Upload failed: ' + data.message, 'danger');
            }
        })
        .catch(err => {
            console.error(err);
            this.showToaster('Network error during upload.', 'danger');
        })
        .finally(() => {
            if (btn) {
                btn.disabled = false;
                btn.innerHTML = '<i class="bi bi-upload"></i> Insert Image';
            }
        });
    }

    // =========================================================================
    // SAVING, PDF GENERATION & EXPORTS
    // =========================================================================

    saveDocument(status = 'Draft') {
        const docId = document.getElementById('currentDocumentId')?.value;
        const docName = document.getElementById('documentNameInput')?.value?.trim() || 'Untitled Document';
        const docType = document.getElementById('documentTypeSelect')?.value || 'Other';

        const payload = {
            DocumentId: docId ? parseInt(docId) : null,
            DocumentName: docName,
            DocumentType: docType,
            TemplateId: null,
            ComponentsJson: JSON.stringify(this.components),
            PrintSettingsJson: JSON.stringify(this.printSettings),
            Status: status
        };

        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');

        this.showToaster('Saving...', 'info');

        fetch('/DocumentBuilder/SaveDocument', {
            method: 'POST',
            body: JSON.stringify(payload),
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': tokenInput?.value || ''
            }
        })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                document.getElementById('currentDocumentId').value = data.documentId;
                this.showToaster('Document saved successfully!', 'success');
            } else {
                this.showToaster('Save failed: ' + data.message, 'danger');
            }
        })
        .catch(err => {
            console.error(err);
            this.showToaster('Network error saving document.', 'danger');
        });
    }

    startAutoSave() {
        this.autoSaveInterval = setInterval(() => {
            const docId = document.getElementById('currentDocumentId')?.value;
            if (docId && this.components.length > 0) {
                this.saveDocument('Draft');
            }
        }, 40000);
    }

    exportPdf() {
        this.deselectAll();
        this.showToaster('Generating PDF...', 'info');

        const docName = document.getElementById('documentNameInput')?.value?.trim() || 'SchoolDocument';
        const paper = document.getElementById('docPaper');

        const { jsPDF } = window.jspdf;
        const orientation = this.printSettings.orientation === 'landscape' ? 'l' : 'p';
        const format = (this.printSettings.pageSize || 'a4').toLowerCase();

        const doc = new jsPDF({
            orientation: orientation,
            unit: 'mm',
            format: format
        });

        const pdfWidth = doc.internal.pageSize.getWidth();
        const pdfHeight = doc.internal.pageSize.getHeight();

        html2canvas(paper, {
            scale: 2,
            useCORS: true,
            allowTaint: true,
            backgroundColor: '#ffffff'
        }).then(canvas => {
            const imgData = canvas.toDataURL('image/png');
            const imgWidth = pdfWidth;
            const imgHeight = (canvas.height * pdfWidth) / canvas.width;

            let heightLeft = imgHeight;
            let position = 0;

            // First page
            doc.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
            heightLeft -= pdfHeight;

            // Additional pages if content overflows
            while (heightLeft > 0) {
                position = -(imgHeight - heightLeft);
                doc.addPage();
                doc.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
                heightLeft -= pdfHeight;
            }

            doc.save(`${docName}.pdf`);
            this.showToaster('PDF exported!', 'success');
        }).catch(err => {
            console.error("PDF generation failed:", err);
            this.showToaster('Failed to export PDF.', 'danger');
        });
    }

    // =========================================================================
    // PANELS, ZOOM & UI HELPERS
    // =========================================================================

    togglePanel(side) {
        if (side === 'left') {
            const panel = document.getElementById('panelLeft');
            if (panel) panel.classList.toggle('collapsed');
        } else if (side === 'right') {
            const panel = document.getElementById('panelRight');
            if (panel) panel.classList.toggle('collapsed');
        }
    }

    zoomCanvas(delta) {
        if (delta === 0) {
            this.zoomLevel = 100; // Reset
        } else {
            this.zoomLevel = Math.max(50, Math.min(200, this.zoomLevel + delta));
        }

        const paper = document.getElementById('docPaper');
        if (paper) {
            paper.style.transform = `scale(${this.zoomLevel / 100})`;
            paper.style.transformOrigin = 'top center';
        }

        const label = document.getElementById('zoomLevel');
        if (label) label.textContent = this.zoomLevel + '%';
    }

    filterComponents(searchText) {
        const cards = document.querySelectorAll('.component-card');
        const query = (searchText || '').toLowerCase().trim();

        cards.forEach(card => {
            const label = card.querySelector('.component-card-label')?.textContent?.toLowerCase() || '';
            card.style.display = label.includes(query) ? '' : 'none';
        });
    }

    toggleComponentGroup(headerEl) {
        const body = headerEl.nextElementSibling;
        const chevron = headerEl.querySelector('.group-chevron');
        if (body) {
            const isOpen = body.style.display !== 'none';
            body.style.display = isOpen ? 'none' : 'block';
            if (chevron) {
                chevron.classList.toggle('bi-chevron-down', !isOpen);
                chevron.classList.toggle('bi-chevron-right', isOpen);
            }
        }
    }

    togglePropGroup(headerEl) {
        const body = headerEl.nextElementSibling;
        const chevron = headerEl.querySelector('.prop-chevron');
        if (body) {
            const isOpen = body.style.display !== 'none';
            body.style.display = isOpen ? 'none' : '';
            if (chevron) {
                chevron.classList.toggle('bi-chevron-down', !isOpen);
                chevron.classList.toggle('bi-chevron-right', isOpen);
            }
        }
    }

    setActiveAlignBtn(clickedBtn) {
        document.querySelectorAll('#textAlignGroup .prop-btn').forEach(b => b.classList.remove('active'));
        clickedBtn.classList.add('active');
    }

    showToaster(msg, type = 'success') {
        // Use Bootstrap toast from View (#builderToast, #toastBody)
        const toastBody = document.getElementById('toastBody');
        const toastEl = document.getElementById('builderToast');

        if (toastBody && toastEl) {
            toastBody.textContent = msg;

            // Color the toast
            toastEl.className = 'toast align-items-center border-0';
            if (type === 'success') toastEl.classList.add('text-bg-success');
            else if (type === 'danger') toastEl.classList.add('text-bg-danger');
            else if (type === 'warning') toastEl.classList.add('text-bg-warning');
            else toastEl.classList.add('text-bg-primary');

            const bsToast = bootstrap.Toast.getOrCreateInstance(toastEl, { delay: 3000 });
            bsToast.show();
        }
    }
}


// =============================================================================
// GLOBAL FUNCTION BRIDGES
//
// The View's inline onclick handlers call these as global functions.
// They delegate to the builder class instance.
// =============================================================================

let builder;

document.addEventListener('DOMContentLoaded', () => {
    builder = new DocumentBuilder();
});

// Toolbar actions
function saveDocument() { builder?.saveDocument('Draft'); }
function exportPdf() { builder?.exportPdf(); }

// Panel & Zoom
function togglePanel(side) { builder?.togglePanel(side); }
function zoomCanvas(delta) { builder?.zoomCanvas(delta); }

// Left panel — Component search & groups
function filterComponents(searchText) { builder?.filterComponents(searchText); }
function toggleComponentGroup(headerEl) { builder?.toggleComponentGroup(headerEl); }

// Right panel — Property groups
function togglePropGroup(headerEl) { builder?.togglePropGroup(headerEl); }

// Properties panel — style updates (called from oninput/onchange on property inputs)
function updateComponentProperty(propName, value) { builder?.updateComponentProperty(propName, value); }

// Properties panel — question data updates
function updateComponentData(dataKey, value) { builder?.updateComponentData(dataKey, value); }

// MCQ options
function updateMcqOption(idx, value) { builder?.updateMcqOption(idx, value); }
function removeMcqOption(idx) { builder?.removeMcqOption(idx); }
function addMcqOption() { builder?.addMcqOption(); }

// Alignment button highlight
function setActiveAlignBtn(btn) { builder?.setActiveAlignBtn(btn); }

// Delete selected component (from properties panel delete button)
function deleteSelectedComponent() { builder?.deleteSelectedComponent(); }

// Templates
function loadTemplate(templateId) { builder?.loadTemplate(templateId); }
function filterTemplates(filterType, btn) { builder?.filterTemplates(filterType, btn); }

// Print Settings
function applyPrintSettings() { builder?.applyPrintSettingsFromModal(); }

// Image Upload
function uploadImage() { builder?.uploadImage(); }

// Context Menu
function contextAction(action) { builder?.contextAction(action); }

// ============================================================
// IMPORT QUESTION BANK MODAL LOGIC
// ============================================================
let importQuestionsList = [];
let selectedImportQuestions = new Set();
let importModalInstance = null;

function openImportModal() {
    if (!importModalInstance) {
        importModalInstance = new bootstrap.Modal(document.getElementById('importQuestionModal'));
    }
    document.getElementById('importFilterType').value = '';
    selectedImportQuestions.clear();
    updateImportSelectionUI();
    loadImportQuestions();
    importModalInstance.show();
}

function loadImportQuestions() {
    const filter = document.getElementById('importFilterType').value;
    const classId = document.getElementById('importClassId').value;
    const subjectId = document.getElementById('importSubjectId').value;
    
    const listContainer = document.getElementById('importQuestionList');
    
    listContainer.innerHTML = '<div class="p-4 text-center text-muted"><div class="spinner-border spinner-border-sm me-2"></div> Loading questions...</div>';
    
    fetch(`/DocumentBuilder/GetQuestionsApi?filterType=${filter}&classId=${classId}&subjectId=${subjectId}`)
        .then(res => res.json())
        .then(res => {
            if (res.success) {
                importQuestionsList = res.data;
                document.getElementById('importQuestionCount').textContent = `${importQuestionsList.length} questions found`;
                renderImportList();
            } else {
                listContainer.innerHTML = `<div class="p-4 text-center text-danger"><i class="bi bi-exclamation-triangle"></i> Failed to load questions.</div>`;
            }
        })
        .catch(err => {
            listContainer.innerHTML = `<div class="p-4 text-center text-danger"><i class="bi bi-wifi-off"></i> Network error.</div>`;
        });
}

function renderImportList() {
    const listContainer = document.getElementById('importQuestionList');
    if (importQuestionsList.length === 0) {
        listContainer.innerHTML = '<div class="p-4 text-center text-muted">No questions found matching this filter.</div>';
        return;
    }

    listContainer.innerHTML = '';
    importQuestionsList.forEach(q => {
        const isSelected = selectedImportQuestions.has(q.questionId);
        
        const item = document.createElement('div');
        item.className = `list-group-item list-group-item-action import-question-item ${isSelected ? 'selected' : ''}`;
        item.onclick = () => toggleImportQuestion(q.questionId, item);
        
        // Strip HTML from QuestionText for a cleaner preview
        const tmp = document.createElement("DIV");
        tmp.innerHTML = q.questionText;
        const textPreview = tmp.textContent || tmp.innerText || "";
        
        item.innerHTML = `
            <div class="d-flex w-100 justify-content-between align-items-center">
                <div class="d-flex align-items-center gap-3">
                    <input class="form-check-input import-question-checkbox mt-0" type="checkbox" ${isSelected ? 'checked' : ''} value="${q.questionId}">
                    <div>
                        <span class="badge bg-secondary mb-1">${q.questionType}</span>
                        <h6 class="mb-1 text-truncate" style="max-width: 500px;">${textPreview}</h6>
                        <small class="text-muted"><i class="bi bi-file-earmark-text"></i> ${q.documentName || 'Manual Entry'}</small>
                    </div>
                </div>
                <div class="text-end">
                    <span class="badge bg-light text-dark border"><i class="bi bi-star-fill text-warning"></i> ${q.marks} Marks</span>
                </div>
            </div>
        `;
        listContainer.appendChild(item);
    });
}

function toggleImportQuestion(id, itemElement) {
    if (selectedImportQuestions.has(id)) {
        selectedImportQuestions.delete(id);
        itemElement.classList.remove('selected');
        itemElement.querySelector('.import-question-checkbox').checked = false;
    } else {
        selectedImportQuestions.add(id);
        itemElement.classList.add('selected');
        itemElement.querySelector('.import-question-checkbox').checked = true;
    }
    updateImportSelectionUI();
}

function updateImportSelectionUI() {
    const count = selectedImportQuestions.size;
    document.getElementById('selectedImportCount').textContent = count;
    document.getElementById('btnInsertQuestions').disabled = (count === 0);
}

function insertImportedQuestions() {
    if (!builder || selectedImportQuestions.size === 0) return;
    
    // Find the actual question objects
    const questionsToInsert = importQuestionsList.filter(q => selectedImportQuestions.has(q.questionId));
    
    questionsToInsert.forEach(q => {
        // Convert QuestionDto format into the builder's Component object format
        const newComp = {
            id: 'comp_' + Date.now() + '_' + Math.floor(Math.random() * 1000),
            type: q.questionType,
            content: q.questionText,
            style: {
                fontFamily: "Arial",
                fontSize: "14px",
                fontWeight: "normal",
                textAlign: "left",
                color: "#000000",
                marginTop: "10px",
                marginBottom: "10px"
            },
            questionProps: {
                qNumber: "", // Let the teacher number them
                marks: q.marks.toString(),
                space: (q.answerSpace || 50).toString()
            },
            order: builder.components.length,
            page: 1 // Append to page 1 by default, builder engine will re-paginate later if needed
        };
        
        // Handle options if it's MCQ
        if (q.questionType === 'mcq' && q.optionsJson) {
            try {
                newComp.questionProps.options = JSON.parse(q.optionsJson);
            } catch(e) {
                newComp.questionProps.options = [{id:'A',text:'Option A'},{id:'B',text:'Option B'}];
            }
        }
        
        builder.components.push(newComp);
    });
    
    importModalInstance.hide();
    builder.renderLoadedComponents(builder.components);
    
    // Show quick toast notification (optional, assuming we have sweetalert)
    if(window.Swal) {
        Swal.fire({
            icon: 'success',
            title: 'Imported!',
            text: `${questionsToInsert.length} questions imported to canvas.`,
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 3000
        });
    }
}
