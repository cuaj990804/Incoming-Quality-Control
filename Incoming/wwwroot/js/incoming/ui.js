import { api } from './api.js';
export const ui = {
    defectTranslations: {
        'DAMAGED CABLE': 'Cable dañado',
        'MOLDING': 'Moldeo',
        'GAP': 'Gap',
        'discoloration': 'Decoloración',
        'short_circuit': 'Corto circuito'

    },
    elements: {
        scannerInput: document.getElementById('ProgramSelect'),
        baseInput: document.getElementById('Base'),
        resultsTable: document.getElementById('resultsTable'),
        programLabel: document.getElementById('ProgramLabel'),
        containerCountLabel: document.getElementById('CointainerCountLabel'),
        rejectionList: document.getElementById('rejection-list'),
        defectsContainer: document.getElementById('defects-container'),
        defectsLabel: document.getElementById('defects-label'),
        tableContainer: document.querySelector('.table-container'),
        tableBody: document.querySelector('.table-container tbody'),
        submitButton: document.getElementById('submit-button'),
        ModalProgramList: document.getElementById('ProgramModalSelect'),
        ModalQuantityInput: document.getElementById('QuantityContainerIn'),
        EditModalProgramList: document.getElementById('EditProgramModalSelect'), 
        EditModalQuantityInput: document.getElementById('EditQuantityContainerIn'),
        JulianInputModal: document.getElementById('JulianContainerIn'),
        BaseInputModal: document.getElementById('BaseContainerIn'),
        tableModal: document.getElementById('tableModal'),
        tableBodyModal: document.getElementById('tableBodyModal'),
        ProgramModal: null,
        PartnumberModal: null          
    },
    state: { 
        currentData: {
            partnumber: null,
            program: null,
            base:null
        }
    },

    clearUI: () => {
        //ui.elements.programLabel.textContent = '';
        //ui.elements.scannerInput.value = '';
        ui.elements.baseInput.value = '';
        ui.elements.submitButton.style.display = 'none';
        ui.ocultarTabla();
        ui.ocultarcheckbox();
        ui.elements.baseInput.focus();
    },
    clearNotable: () => {
        //ui.elements.programLabel.textContent = '';
        //ui.elements.scannerInput.value = '';
        ui.elements.baseInput.value = '';


        ui.elements.baseInput.focus();
    },

    showAlert: (icon, title, text) => {
        return Swal.fire({
            icon, title, text, showConfirmButton: false, timer: 2000, customClass: {
                popup: 'custom-swal-popup',
                title: 'custom-swal-title',
                htmlContainer: 'custom-swal-content',
                confirmButton: 'custom-swal-button'
            } });
    },

    
    showAlertError: (icon, title, text) => {
        Swal.fire({
            icon, title, text, showConfirmButton: true, customClass: {
                popup: 'custom-swal-popup',
                title: 'custom-swal-title',
                htmlContainer: 'custom-swal-content',
                confirmButton: 'custom-swal-button'
            } });
    },

    updateProgramLabel: async (programName) => {
        try { 
            ui.elements.programLabel.textContent = programName;
            
            const result = await api.getContainerCounts(programName); // Agregar await
            const { PartialCount, Quantity } = result; // Usar PascalCase
            ui.elements.containerCountLabel.textContent = `${PartialCount} escaneados de ${Quantity}`;
        } catch (error) {
            console.error("Error actualizando etiqueta:", error);
            ui.elements.programLabel.textContent = `${programName} 0 de 0`;
        }
    },


    createDefectsCheckboxes: (defects, parameters) => {
        const container = document.createElement('div');
        container.className = 'defects-checkboxes';
        ui.elements.defectsContainer.style.display = 'block';
        const form = document.createElement('form');

        // Título
        const title = document.createElement('label');
        title.textContent = 'Selecciona el tipo de defectos:';
        title.className = 'defect-title';
        form.appendChild(title);

        // Contenedor para checkboxes
        const checkboxesContainer = document.createElement('div');
        checkboxesContainer.className = 'checkboxes-container';

        defects.forEach((defect) => {
            const checkboxWrapper = document.createElement('div');
            checkboxWrapper.className = 'checkbox-wrapper';

            const checkbox = document.createElement('input');
            checkbox.type = 'checkbox';
            checkbox.className = 'form-check-input defect-checkbox';
            checkbox.id = `defect-${defect.id}`;
            checkbox.name = 'defects';
            checkbox.value = defect.defectName;

            const label = document.createElement('label');
            label.htmlFor = checkbox.id;
            // Usar la traducción o el nombre original si no existe traducción
            label.textContent = ui.defectTranslations[defect.defectName] || defect.defectName;
            label.className = 'defect-label';

            checkboxWrapper.appendChild(checkbox);
            checkboxWrapper.appendChild(label);
            checkboxesContainer.appendChild(checkboxWrapper);
        });


        form.appendChild(checkboxesContainer);
        container.appendChild(form);

        // Evento change modificado
        form.addEventListener('change', (e) => {
            if (e.target.classList.contains('defect-checkbox')) {
                const selectedDefects = [...form.querySelectorAll('.defect-checkbox:checked')];
                const hasDamagedCable = selectedDefects.some(checkbox =>
                    checkbox.value === 'DAMAGED CABLE' // Verificar si hay Cable dañado

                );


                if (hasDamagedCable) {
                    ui.ocultarTabla(); // Ocultar solo si está Cable dañado

                   
                } else {
                    // Mostrar todos los checkboxes
                    
                    ui.actualizarTabla(parameters, selectedDefects); // Mostrar tabla en otros casos
                }
            }
        });

        return container;
    },

    actualizarTabla: (parameters, selectedDefects) => {
        if (!parameters?.orderedResult?.length) return;

        // Mostrar tabla siempre que se llame a esta función
        ui.elements.tableContainer.style.display = 'flex';

        ui.elements.tableBody.innerHTML = parameters.orderedResult.map(test => {
            const showTablaNTC = test.minValue == 0 && test.maxValue == 0;
            return `
        <tr>
            <td><input type="checkbox" class="form-check-input" checked></td>
            <td>${test.test}</td>
            <td>${showTablaNTC ? 'VER TABLA DE TEMPERATURA' : test.minValue}</td>
            <td>${showTablaNTC ? 'VER TABLA DE TEMPERATURA' : test.maxValue}</td>
            <td>${selectedDefects.length ? 'N/A' : 'OK'}</td>
        </tr>
    `;
        }).join('');


    },

    ocultarTabla: () => {
        ui.elements.tableContainer.style.display = 'none';
        ui.elements.tableBody.innerHTML = '';

    },
    ocultarcheckbox: () => {
        ui.elements.defectsContainer.style.display = 'none';
    },

    setupTableEvents: () => {
        ui.elements.tableContainer.addEventListener('change', (event) => {
            const checkbox = event.target;
            if (!checkbox.classList.contains('form-check-input')) return;

            const row = checkbox.closest('tr');
            const valorCell = row.querySelector('td:nth-child(5)');

            if (checkbox.checked) {
                valorCell.innerHTML = 'OK';
            } else {
                valorCell.innerHTML = '<input type="text" class="form-control input-value" placeholder="OVERFLOW">';
                const input = valorCell.querySelector('input');

                input.focus();
                input.addEventListener('input', () => {
                    if (input.value.trim()) checkbox.checked = false;
                });
                
            }
        });
    },
    updateRejectionList: (counts) => {
        const container = ui.elements.rejectionList;

        if (!counts || counts.length === 0) {
            container.innerHTML = `
            <div class="no-rejections">
                No hay material rechazado el día de hoy
            </div>
        `;
            return;
        }

        container.innerHTML = counts.map(c => `
        <div class="rejection-item">
            <span class="program-name">${c.programName}:</span>
            <span class="rejection-count">${c.count}</span>
        </div>
    `).join('');
    },
    eventCheckbox: (container) => {
        container.addEventListener('change', (event) => {
            const checkbox = event.target;
            if (!checkbox.classList.contains('form-check-input')) return;

            const row = checkbox.closest('tr');
            const valorCell = row.querySelector('td:nth-child(5)');

            if (checkbox.checked) {
                valorCell.innerHTML = 'OK';
            } else {
                valorCell.innerHTML = '<input type="text" class="form-control input-value" placeholder="OVERFLOW">';
                const input = valorCell.querySelector('input');

                input.focus();
                input.addEventListener('input', () => {
                    if (input.value.trim()) checkbox.checked = false;
                });
            }
        });
    },
    eventCheckboxModal: (table) => {
        const checkboxes = table.querySelectorAll(".inputTableModal");

        checkboxes.forEach(checkbox => {
            checkbox.addEventListener("change", () => {
                const row = checkbox.closest("tr");
                const valorCell = row.querySelectorAll("td")[4];

                if (!checkbox.checked) {
                    valorCell.innerHTML = `<input type="text" class="form-control form-control-sm" placeholder="    OVERFLOW">`;
                } else {
                    valorCell.innerHTML = "OK";
                }
            });
        });
    },


    ShowTableModal: (parameters) => {
        if (!parameters?.orderedResult?.length) return;

        ui.elements.tableModal.innerHTML = ""; // Limpiar antes de volver a insertar

        const tableHTML = `
        <table class="table table-sm">
            <thead>
                <tr>
                    <th></th>
                    <th>Prueba</th>
                    <th>Mínimo</th>
                    <th>Máximo</th>
                    <th>Valor</th>
                </tr>
            </thead>
            <tbody>
                ${parameters.orderedResult.map(test => {
            const showTablaNTC = test.minValue == 0 && test.maxValue == 0;
            return `
                        <tr>
                            <td><input type="checkbox" class="form-check-input inputTableModal" checked></td>
                            <td>${test.test}</td>
                            <td>${showTablaNTC ? 'VER TABLA DE TEMPERATURA' : test.minValue}</td>
                            <td>${showTablaNTC ? 'VER TABLA DE TEMPERATURA' : test.maxValue}</td>
                            <td>OK</td>
                        </tr>
                    `;
        }).join('')}
            </tbody>
        </table>
    `;

        ui.elements.tableModal.innerHTML = tableHTML;
        ui.eventCheckboxModal(ui.elements.tableModal);
    }



};