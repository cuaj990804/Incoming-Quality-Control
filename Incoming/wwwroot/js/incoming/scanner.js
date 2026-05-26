import { api } from './api.js';
import { ui } from './ui.js';
import { events } from './events.js';

export const scanner = {

    init: () => {
        ui.elements.baseInput.addEventListener('input', scanner.handleInput);
        ui.elements.scannerInput.addEventListener('change', scanner.handleScannerEnter);
        ui.elements.baseInput.addEventListener('keypress', scanner.handleBaseEnter);
        ui.elements.JulianInputModal.addEventListener('keypress', scanner.handleJulianModalEnter);
        ui.elements.BaseInputModal.addEventListener('keypress', scanner.handleBaseModalEnter);
        ui.setupTableEvents();
    },

    handleInput: ({ target }) => {
        if (!target.value) ui.clearUI();
    },

    handleScannerEnter: async (event) => {
        const ProgramValue = ui.elements.scannerInput.value.trim();
        ui.elements.baseInput.focus();

        
        const { Status } = await api.getContainerStatus(ProgramValue);
       
        // ui.showAlert('success', 'Registro Exitoso', 'El volante fue escaneado con éxito');
        


        try {
            const parameters = await api.getPartDetails(ProgramValue);
            const data = await api.getDefects();
            if (Status === 'CLOSE') {
                events.showContainerModal();
                ui.clearUI();
                return
            } else {
                if (Array.isArray(data) && data.length > 0) {
                    scanner.handleDefects(data, parameters);
                    ui.elements.submitButton.style.display = 'block'; // Mostrar botón
                } else {
                    ui.showAlertError('error', 'Sin resultados', 'Intenta con otro número de parte');
                    ui.clearUI();
                }
            }

            
        } catch (error) {
            ui.showAlertError('error', 'Error', error.message);
            ui.clearUI();
        }
        
    },

    // Nuevo: Manejar Enter en baseInput para procesar datos
    handleBaseEnter: async (event) => {
      /*  if (event.key === 'Enter') {
            event.preventDefault();
            
            const baseValue = ui.elements.baseInput.value.trim();

            if (!baseValue) {
                ui.showAlertError('error', 'Error', 'La base es requerida');
                ui.clearUI();
                return;
            }

            try {
              
                const program = ui.elements.programLabel.textContent;
                //api.UpdatePartialCount(program);

                
            } catch (error) {
                ui.showAlertError('error', 'Error', error.message);
                ui.clearUI();
            }
        }*/
    },
    handleJulianModalEnter: async (event) => {
        
        ui.elements.BaseInputModal.focus();
        

    },
    handleBaseModalEnter: async (event) => {
        if (event.key === 'Enter') {
            const Julian = ui.elements.JulianInputModal.value.trim();
            const base = ui.elements.BaseInputModal.value.trim() + Julian;

            // Mostrar SweetAlert2 con spinner
            Swal.fire({
                title: 'Cargando...',
                text: 'Consultando información del número de parte',
                allowOutsideClick: false,
                allowEscapeKey: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            try {
                const response = await fetch(`http://192.168.1.1:9090/api/labels?partnumber=${encodeURIComponent(Julian)}`);

                if (!response.ok) {
                    const errorData = await response.json();
                    throw new Error(errorData.message || "Error en la consulta");
                }

                const data = await response.json();

                const filteredData = data.map(item => ({
                    partnumber: item.partnumber,
                    partnumberProgram: item.partnumberProgram
                }));

                if (filteredData.length > 0) {
                    const { partnumber, partnumberProgram } = filteredData[0];
                    ui.state.currentData = {
                        partnumber: data[0].partnumber,
                        program: data[0].partnumberProgram,
                        base: base
                    };

                    const parameters = await api.getPartDetails(partnumberProgram);
                    ui.ShowTableModal(parameters);
                    console.log(parameters);
                }

                Swal.close(); // Cerrar el loading

            } catch (error) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: error.message
                });
                console.error("Error en la solicitud:", error);
            }
        }             

    },


    handleDefects: (data, parameters) => {

        const existingCheckboxes = ui.elements.defectsContainer.querySelector('.defects-checkboxes');
        if (existingCheckboxes) existingCheckboxes.remove();

        const checkboxes = ui.createDefectsCheckboxes(data, parameters);
        ui.elements.defectsContainer.prepend(checkboxes);
        ui.updateProgramLabel(parameters.orderedResult[0].programName);
        ui.storedTests = parameters.orderedResult.map(p => p.test);
        ui.allDefects = data.map(defect => defect.defectName);

        ui.actualizarTabla(parameters, []);
    }

};