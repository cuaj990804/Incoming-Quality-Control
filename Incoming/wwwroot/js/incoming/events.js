//Asigna los eventos a los elementos de la interfaz(por ejemplo, botones de escaneo).
import { api } from './api.js';
import { ui } from './ui.js';
import { scanner } from './scanner.js';



const modalElement = document.getElementById('ContainerModal');
const modal = new bootstrap.Modal(modalElement);

const editModalElement = document.getElementById('EditContainerModal');
const editModal = new bootstrap.Modal(editModalElement);

const selectActionModalElement = document.getElementById('SelectActionModal');
const selectActionModal = new bootstrap.Modal(selectActionModalElement);

const SteeringWheelModalElement = document.getElementById('SteeringWheelContainerModal');
const SteeringWheelContainerModal = new bootstrap.Modal(SteeringWheelModalElement);

const editDefectModalElement = document.getElementById('EditDefectModal');
const editDefectModal = new bootstrap.Modal(editDefectModalElement);





export const events = {
    init: () => {
        document.querySelector('.btn-primary').addEventListener('click', events.submitData);

        document.getElementById('AddContainer').addEventListener('click', events.AddContainer);

        document.getElementById('EditContainer').addEventListener('click', events.UpdateContainerQuantity);
        document.getElementById('AddSteeringWheel').addEventListener('click', events.insertSteeringWheelData);

        document.getElementById('openSelectActionModal').addEventListener('click', events.showSelectActionModal);
        document.getElementById('EditProgramModalSelect').addEventListener('change', events.updateQuantityBasedOnProgram);
        document.getElementById('OpenSteeringWheelModal').addEventListener('click', events.showSteeringWheelContainerModal);

        document.getElementById('saveDefectChanges').addEventListener('click', events.saveDefectChanges);

        //modalElement.addEventListener('hidden.bs.modal', events.showSelectActionModal);
        //editModalElement.addEventListener('hidden.bs.modal', events.showSelectActionModal);


        document.addEventListener('click', function (e) {
            if (e.target && e.target.id === 'openContainerModal') {
                events.showContainerModal();
            }
        });

        document.addEventListener('click', function (e) {
            if (e.target && e.target.id === 'OpenEditContainer') {
                events.showEditContainerModal();
            }
        });





                 
    },
    showSelectActionModal: () => {
        selectActionModal.show();
    },
    showContainerModal: () => {
        selectActionModal.hide();
        modal.show();

        // Opcional: Resetear el formulario al abrir
        document.getElementById('ProgramModalSelect').value = '';
        document.getElementById('QuantityContainerIn').value = '';
    },
    showEditContainerModal: () => {
        selectActionModal.hide();
        editModal.show();
        document.getElementById('EditProgramModalSelect').value = '';
        document.getElementById('EditQuantityContainerIn').value = '';
    },
    showSteeringWheelContainerModal: () => {
        selectActionModal.hide(); 
        SteeringWheelContainerModal.show();
        setTimeout(() => {
            ui.elements.JulianInputModal.focus();
        }, 1000);
        

        // Opcional: limpiar campos del modal
        document.getElementById('JulianContainerIn').value = '';
        document.getElementById('BaseContainerIn').value = '';
    },



  
    hideContainerModal: () => {
        

        // Opcional: resetea al ocultar
        ui.elements.ModalProgramList.value = '';
        ui.elements.ModalQuantityInput.value = '';

        modal.hide();
    },
    submitData: async () => {
        const baseValue = ui.elements.baseInput.value.trim();
        if (!baseValue) {
            ui.showAlert('error', 'Error', 'La base es requerida');
            ui.elements.baseInput.focus();
            return;
        }

        const program = ui.elements.programLabel.textContent.trim();

        const allDefects = ui.allDefects;
        const selectedDefects = new Set(
            [...document.querySelectorAll('.defect-checkbox:checked')].map(cb => cb.value)
        );
        const hasDamagedCable = selectedDefects.has('DAMAGED CABLE');

        const defectos = hasDamagedCable
            ? allDefects.map(d => d === 'DAMAGED CABLE' ? 'DEFECT' : 'N/A')
            : allDefects.map(d => selectedDefects.has(d) ? 'DEFECT' : 'OK');

        const hasRealDefects = !hasDamagedCable && defectos.includes('DEFECT');

        const parametros = hasDamagedCable || hasRealDefects
            ? ui.storedTests.map(prueba => ({ prueba, valor: 'N/A' }))
            : [...ui.elements.tableBody.querySelectorAll('tr')].map(row => {
                const prueba = row.cells[1].textContent;
                const checkbox = row.querySelector('input[type="checkbox"]');
                const inputValor = row.querySelector('.input-value');
                return {
                    prueba,
                    valor: checkbox.checked
                        ? 'OK'
                        : (inputValor?.value || 'OVERFLOW')
                };
            });

        const dbData = {
            PROGRAM: program,
            BASE: baseValue,
            MOLDING: defectos[1],
            GAP: defectos[2],
            CONNECTOR: defectos[0],
            ...parametros.reduce((acc, { prueba, valor }) => {
                acc[prueba] = valor;
                return acc;
            }, {})
        };

        try {
            // 👇 Primero insertamos la información (asegura registrar la pieza)
            if (hasDamagedCable || hasRealDefects || parametros.some(p => p.valor !== 'OK')) {
                await api.insertRejectionData(dbData);
            } else {
                // 👇 Si no hay defectos, insertamos en la tabla de volantes aceptados
                await api.insertAcceptedData(dbData);
            }

            // 👇 Luego actualizamos el conteo
            const { status } = await api.UpdatePartialCount(program);

            // 👇 Si todo está bien, limpiamos
            if (!hasDamagedCable && !hasRealDefects && parametros.every(p => p.valor === 'OK')) {
                ui.clearNotable();
                await ui.showAlert('success', 'Registro Exitoso', 'El volante fue escaneado con éxito');

            }
           

            // 👇 Si ya se cerró el contenedor, mostramos el modal DESPUÉS de registrar
            
            // Preparamos el siguiente ciclo
            scanner.handleScannerEnter();
            await ui.updateProgramLabel(program);
            ui.updateRejectionList();
            api.getRejectionCounts()
                .then(ui.updateRejectionList)
                .catch(console.error);
            if (status === 'CLOSE') {
                events.showContainerModal();
                return;
            }

            ui.clearUI();
            if (hasDamagedCable || hasRealDefects || parametros.some(p => p.valor !== 'OK')) {
                ui.showAlert('success', 'Registro Exitoso', 'El volante fue escaneado con éxito (con defectos)');
            }

        } catch (error) {
            // Manejar diferentes tipos de conflictos
            if (error.conflictData) {
                if (error.conflictType === 'rejection') {
                    // Volante previamente rechazado
                    events.showDefectConflictModal(error.conflictData, dbData);
                } else if (error.conflictType === 'accepted') {
                    // Volante ya aceptado anteriormente
                    events.showAcceptedConflictModal(error.conflictData);
                }
            } else {
                ui.showAlertError('error', 'Error', error.message);
                ui.clearNotable();
            }
        }
    },

    showAcceptedConflictModal: (conflictData) => {
        const acceptedDate = new Date(conflictData.acceptedDate);
        const formattedDate = acceptedDate.toLocaleString('es-MX', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit',
            hour12: true
        });

        Swal.fire({
            title: 'Volante Ya Registrado',
            html: `
                <div style="text-align: center;">
                    <p style="font-size: 18px; margin-bottom: 15px;">
                        <i class="fas fa-check-circle" style="color: #28a745; font-size: 50px;"></i>
                    </p>
                    <p style="font-size: 16px; margin-bottom: 10px;">
                        Este volante ya fue escaneado <strong>sin defectos</strong> anteriormente.
                    </p>
                    <div style="background-color: #d4edda; padding: 15px; border-radius: 8px; border: 1px solid #c3e6cb; margin: 20px 0;">
                        <p style="margin: 5px 0;"><strong>Fecha de escaneo:</strong></p>
                        <p style="margin: 5px 0; font-size: 18px; color: #155724;">${formattedDate}</p>
                    </div>
                    <p style="color: #6c757d; font-size: 14px;">
                        El volante ya está registrado como pieza aceptable.
                    </p>
                </div>
            `,
            icon: 'info',
            confirmButtonColor: '#007bff',
            confirmButtonText: 'Entendido',
            customClass: {
                popup: 'custom-swal-popup',
                title: 'custom-swal-title',
                htmlContainer: 'custom-swal-content',
                confirmButton: 'custom-swal-button'
            },
            width: '500px'
        }).then(() => {
            // Limpiar y preparar para el siguiente escaneo
            ui.clearNotable();
            scanner.handleScannerEnter();
        });
    },

    showDefectConflictModal: (conflictData, currentData) => {
        const defects = conflictData.defects;
        const rejectionDate = new Date(conflictData.rejectionDate);
        const formattedDate = rejectionDate.toLocaleString('es-MX', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit',
            hour12: true
        });

        // Crear lista de defectos encontrados
        let defectsList = '<div style="text-align: left; margin: 20px 0;"><strong>Defectos registrados:</strong><ul style="margin-top: 10px;">';

        const defectLabels = {
            molding: 'Moldeo',
            gap: 'GAP',
            connector: 'Conector',
            heater: 'Heater',
            ntc: 'NTC',
            inside: 'Inside',
            outside: 'Outside',
            guard: 'Guard'
        };

        for (const [key, label] of Object.entries(defectLabels)) {
            const value = defects[key];
            if (value && value !== 'OK' && value !== 'N/A') {
                defectsList += `<li><strong>${label}:</strong> ${value}</li>`;
            }
        }

        defectsList += '</ul></div>';

        Swal.fire({
            title: 'Volante Previamente Rechazado',
            html: `
                <div style="text-align: center;">
                    <p><strong>Base:</strong> ${conflictData.baseNumber}</p>
                    <p><strong>Programa:</strong> ${conflictData.program}</p>
                    <p><strong>Fecha de rechazo:</strong> ${formattedDate}</p>
                </div>
                ${defectsList}
                <div style="margin-top: 20px; text-align: center;">
                    <p style="font-size: 16px; color: #856404; background-color: #fff3cd; padding: 10px; border-radius: 5px; border: 1px solid #ffeaa7;">
                        ¿Qué desea hacer con este volante?
                    </p>
                </div>
            `,
            icon: 'warning',
            showCancelButton: true,
            showDenyButton: true,
            confirmButtonColor: '#28a745',
            denyButtonColor: '#ffc107',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Está OK ahora - Marcar como Aceptado',
            denyButtonText: 'Editar Defectos',
            cancelButtonText: 'Cancelar',
            customClass: {
                popup: 'custom-swal-popup',
                title: 'custom-swal-title',
                htmlContainer: 'custom-swal-content',
                confirmButton: 'custom-swal-button',
                denyButton: 'custom-swal-button',
                cancelButton: 'custom-swal-button'
            },
            width: '600px'
        }).then(async (result) => {
            if (result.isConfirmed) {
                // Usuario confirmó que el volante ahora está OK
                try {
                    await api.convertRejectionToAccepted(currentData);

                    // Actualizar conteo
                    const program = ui.elements.programLabel.textContent.trim();
                    const { status } = await api.UpdatePartialCount(program);

                    ui.clearNotable();
                    await ui.showAlert('success', 'Registro Exitoso', 'El volante fue movido a piezas aceptadas');

                    scanner.handleScannerEnter();
                    await ui.updateProgramLabel(program);
                    ui.updateRejectionList();
                    api.getRejectionCounts()
                        .then(ui.updateRejectionList)
                        .catch(console.error);

                    if (status === 'CLOSE') {
                        events.showContainerModal();
                        return;
                    }

                    ui.clearUI();
                } catch (error) {
                    ui.showAlertError('error', 'Error', error.message);
                    ui.clearNotable();
                }
            } else if (result.isDenied) {
                // Usuario quiere editar los defectos - cargar modal de edición
                events.showEditDefectModal(conflictData.rejectionId);
            } else {
                // Usuario canceló
                ui.clearNotable();
                scanner.handleScannerEnter();
            }
        });
    },

    showEditDefectModal: async (defectId) => {
        try {
            // Obtener los datos del defecto
            const defectData = await api.getDefectData(defectId);

            // Llenar el formulario con los datos
            document.getElementById('editDefectId').value = defectData.id;
            document.getElementById('editProgram').value = defectData.program || '';
            document.getElementById('editBase').value = defectData.baseNumber || '';
            document.getElementById('editMolding').value = defectData.molding || '';
            document.getElementById('editGap').value = defectData.gap || '';
            document.getElementById('editConnector').value = defectData.connector || '';
            document.getElementById('editHeater').value = defectData.heater || '';
            document.getElementById('editNtc').value = defectData.ntc || '';
            document.getElementById('editInside').value = defectData.inside || '';
            document.getElementById('editOutside').value = defectData.outside || '';
            document.getElementById('editGuard').value = defectData.guard || '';

            // Mostrar el modal
            editDefectModal.show();
        } catch (error) {
            ui.showAlertError('error', 'Error', error.message);
        }
    },

    saveDefectChanges: async () => {
        try {
            const defectData = {
                Id: parseInt(document.getElementById('editDefectId').value),
                Program: document.getElementById('editProgram').value,
                Base: document.getElementById('editBase').value,
                Molding: document.getElementById('editMolding').value,
                Gap: document.getElementById('editGap').value,
                Connector: document.getElementById('editConnector').value,
                Heater: document.getElementById('editHeater').value,
                Ntc: document.getElementById('editNtc').value,
                Inside: document.getElementById('editInside').value,
                Outside: document.getElementById('editOutside').value,
                Guard: document.getElementById('editGuard').value
            };

            await api.updateDefect(defectData);

            // Cerrar el modal
            editDefectModal.hide();

            // Mostrar mensaje de éxito
            await ui.showAlert('success', 'Éxito', 'Defecto actualizado correctamente');

            // Actualizar la lista de rechazos
            ui.updateRejectionList();
            api.getRejectionCounts()
                .then(ui.updateRejectionList)
                .catch(console.error);

            // Limpiar y preparar para el siguiente escaneo
            ui.clearNotable();
            scanner.handleScannerEnter();
        } catch (error) {
            ui.showAlertError('error', 'Error', error.message);
        }
    },
    updateQuantityBasedOnProgram: async () => {
        const selectedProgram = document.getElementById('EditProgramModalSelect').value;

        if (!selectedProgram) return; // No hacer nada si no hay selección

        try {
            const result = await api.getContainerQuantity(selectedProgram);

            // Actualiza el input con la cantidad recibida
            document.getElementById('EditQuantityContainerIn').value = result.Quantity;
        } catch (error) {
            console.error("Error updating quantity:", error);
            document.getElementById('EditQuantityContainerIn').value = 0; // Opcional: poner 0 si hay error
        }
    },






    AddContainer: async () => {
        const program = ui.elements.ModalProgramList.value.trim();
        const quantity = parseInt(ui.elements.ModalQuantityInput.value.trim());
        if (!program || isNaN(quantity)) {
            ui.showAlert('error', 'Error', 'Por favor completa todos los campos del contenedor.');
            return;
        }
        const dbData = {
            PROGRAM: program,
            QUANTITY: quantity,
            
        };

      

        try {
            await api.insertContainerData(dbData);
            
            ui.elements.scannerInput.value = program;
            const parameters = await api.getPartDetails(program);
            const data = await api.getDefects();
            if (Array.isArray(data) && data.length > 0) {
                scanner.handleDefects(data, parameters);
                ui.elements.submitButton.style.display = 'block'; // Mostrar botón
            } else {
                ui.showAlertError('error', 'Sin resultados', 'Intenta con otro número de parte');
                ui.clearUI();
            }
            ui.showAlert('success', 'Registro Exitoso', 'El contenedor fue registrado con exito.');

            events.hideContainerModal();
            ui.clearNotable();
            ui.elements.baseInput.focus();
            
        } catch (error) {
            ui.showAlertError('error', 'Error', error.message);
            ui.clearNotable();
        }
    },
    UpdateContainerQuantity: async () => {
        const program = ui.elements.EditModalProgramList.value.trim();
        const quantity = parseInt(ui.elements.EditModalQuantityInput.value.trim());
        console.log(program, quantity);
        if (!program || isNaN(quantity)) {
            ui.showAlert('error', 'Error', 'Por favor completa todos los campos del contenedor.');
            return;
        }
       



        try {
            await api.updateContainerQuantity(program, quantity); 
            
            ui.showAlert('success', 'Registro Exitoso', 'La cantidad fue actualizado con exito.');
            setTimeout(() => {
                location.reload();
            }, 2000);
          

        } catch (error) {
            ui.showAlertError('error', 'Error', error.message);
            ui.clearNotable();
        }
    },
    insertSteeringWheelData: async () => {
        const { partnumber, program, base } = ui.state.currentData;

        const julian = ui.elements.JulianInputModal?.value.trim();
        const baseInput = ui.elements.BaseInputModal?.value.trim();

        // Validar que ambos inputs tengan datos
        if (!julian || !baseInput) {
            ui.showAlertError('error', 'Error', 'Debe llenar ambos campos: Julian y Base.');
            return;
        }

        const table = ui.elements.tableModal;
        const rows = table.querySelectorAll("tbody tr");

        let allOk = true;

        const parameters = Array.from(rows).reduce((acc, row) => {
            const checkbox = row.querySelector("input[type='checkbox']");
            const cells = row.cells;

            const name = cells[1].textContent.trim(); // nombre del parámetro
            const valorCell = cells[4];
            let value;

            if (checkbox.checked) {
                value = "OK";
            } else {
                const input = valorCell.querySelector("input");
                value = input ? (input.value.trim() || "OVERFLOW") : "OVERFLOW";
            }

            if (value !== "OK") allOk = false;
            acc[name] = value;
            return acc;
        }, {});

        // Si todos los valores son OK, limpiar sin insertar
        if (allOk) {
            setTimeout(() => {
                ui.elements.JulianInputModal.value = "";
                ui.elements.BaseInputModal.value = "";
                ui.elements.JulianInputModal.focus();
            }, 1000);
            ui.elements.tableModal.innerHTML = "";
            ui.state.currentData = {};
            return;
        }

        const dataToSend = {
            program,
            partnumber,
            base,
            ...parameters
        };

        try {
            const result = await api.insertSteeringWheelData(dataToSend);
            ui.showAlert(
                'success',
                'Éxito',
                result.message || 'Volante escaneado correctamente'
            );
            setTimeout(() => {
                ui.elements.JulianInputModal.value = "";
                ui.elements.BaseInputModal.value = "";
                ui.elements.JulianInputModal.focus();
            }, 3000);

            if (ui.elements.programInput) ui.elements.programInput.value = "";
            if (ui.elements.partnumberInput) ui.elements.partnumberInput.value = "";

            ui.elements.tableModal.innerHTML = "";
            ui.state.currentData = {};
        } catch (error) {
            console.error("Error:", error);
            ui.showAlertError('error', 'Error', error.message, () => {
                setTimeout(() => {
                    ui.elements.JulianInputModal.value = "";
                    ui.elements.BaseInputModal.value = "";
                    ui.elements.JulianInputModal.focus();
                }, 3000);
            });

            ui.elements.tableModal.innerHTML = "";
            ui.state.currentData = {};
        }
    },




    
};