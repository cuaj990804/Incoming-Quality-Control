// main.js - Inicialización principal
import { api } from './api.js';

function setInputsDisabled(disabled) {
    document.getElementById("MinHEATER").disabled = disabled;
    document.getElementById("MaxHEATER").disabled = disabled;
    document.getElementById("MinNTC").disabled = disabled;
    document.getElementById("MaxNTC").disabled = disabled;
    document.getElementById("MinINSIDE").disabled = disabled;
    document.getElementById("MinOUTSIDE").disabled = disabled;
    document.getElementById("MaxINSIDE").disabled = disabled;
    document.getElementById("MaxOUTSIDE").disabled = disabled;
    document.getElementById("MinGUARD").disabled = disabled;
    document.getElementById("MaxGUARD").disabled = disabled;
}
window.addEventListener("DOMContentLoaded", () => {
    setInputsDisabled(true);
});

let globalProgramData = {};
document.getElementById("ProgramSelect").addEventListener("change", async function () {
    
    const selectedProgram = this.value;
    console.log(selectedProgram);
    setInputsDisabled(false);

    try {
        const parameters = await api.GetParameters(selectedProgram);


        // Crear un diccionario del tipo: { "HEATER": { min: ..., max: ... }, ... }
        const programData = {};
        parameters.forEach(p => {
            programData[p.test.toUpperCase()] = {
                min: p.min,
                max: p.max
            };
        });
        globalProgramData = programData;

        if (programData.HEATER) {
            document.getElementById("MinHEATER").value = programData.HEATER.min;
            document.getElementById("MaxHEATER").value = programData.HEATER.max;
        }

        if (programData.NTC) {
            document.getElementById("MinNTC").value = programData.NTC.min;
            document.getElementById("MaxNTC").value = programData.NTC.max;
        }

        if (programData.INSIDE) {
            document.getElementById("MinINSIDE").value = programData.INSIDE.min;
            document.getElementById("MaxINSIDE").value = programData.INSIDE.max;
        }

        if (programData.OUTSIDE) {
            document.getElementById("MinOUTSIDE").value = programData.OUTSIDE.min;
            document.getElementById("MaxOUTSIDE").value = programData.OUTSIDE.max;
        }

        if (programData.GUARD) {
            document.getElementById("MinGUARD").value = programData.GUARD.min;
            document.getElementById("MaxGUARD").value = programData.GUARD.max;
        }

    } catch (error) {
        console.error("Error al obtener parámetros:", error);
    }
});

//Update
document.getElementById("SaveParameters").addEventListener("click", async function () {
    const selectedProgram = document.getElementById("ProgramSelect").value;

    if (!selectedProgram) {
        alert("Faltan datos.");
        return;
    }

    // 1. Obtener el ID del programa (fuera del bucle)

    const programId = await api.GetProgramId(selectedProgram);
    console.log(programId);
   
  

    const tests = ["HEATER", "NTC", "INSIDE", "OUTSIDE", "GUARD"];
    const updatedParams = [];

    tests.forEach(test => {
        const minInput = document.getElementById(`Min${test}`);
        const maxInput = document.getElementById(`Max${test}`);

        const currentMin = parseFloat(minInput.value) || 0;
        const currentMax = parseFloat(maxInput.value) || 0;

        const original = globalProgramData[test];

        // Verificar cambios
        if (original && (original.min !== currentMin || original.max !== currentMax)) {
            updatedParams.push({
                programId: programId, // ID REAL obtenido previamente
                test,
                minimum: currentMin,
                maximum: currentMax
            });
        }
    });

    if(updatedParams.length === 0) {
        alert("No hay cambios para guardar.");
        return;
    }

    try {
        const response = await api.UpdateParameters(updatedParams);
        console.log("Respuesta del servidor:", response);

        if (response.success) {
            await Swal.fire({
                icon: 'success',
                title: 'Éxito',
                text: response.message,
                confirmButtonColor: '#3498db'
            });
            location.reload();
        } else {
                await Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: response.message,
                    confirmButtonColor: '#3498db'
                });
        }
    } catch (error) {
        console.error("Error al actualizar parámetros:", error);
        alert("Error de conexión: " + error.message);
    }
});


//create logic

document.getElementById("addParameters").addEventListener("click", async function () {
    try {
        const program = document.getElementById("AddProgram").value.trim().toUpperCase();
        if (!program) {
            await Swal.fire({
                icon: 'warning',
                title: 'Campo vacío',
                text: 'Por favor ingrese un nombre de programa'
            });
            return;
        }

        await api.InsertProgram(program);
        const programId = await api.GetProgramId(program);

        const tests = [
            { name: "HEATER", minId: "AddMinHEATER", maxId: "AddMaxHEATER" },
            { name: "NTC", minId: "AddMinNTC", maxId: "AddMaxNTC" },
            { name: "INSIDE", minId: "AddMinINSIDE", maxId: "AddMaxINSIDE" },
            { name: "OUTSIDE", minId: "AddMinOUTSIDE", maxId: "AddMaxOUTSIDE" },
            { name: "GUARD", minId: "AddMinGUARD", maxId: "AddMaxGUARD" }
        ];

        for (const test of tests) {
            const minInput = document.getElementById(test.minId);
            const maxInput = document.getElementById(test.maxId);

            const minValue = parseFloat(minInput.value.trim() || "0");
            const maxValue = parseFloat(maxInput.value.trim() || "1");

            if (isNaN(minValue) || isNaN(maxValue)) {
                throw new Error(`Valores numéricos inválidos para ${test.name}`);
            }

            if (minValue > maxValue) {
                throw new Error(`El valor mínimo no puede ser mayor que el máximo en ${test.name}`);
            }

            await api.InsertTestParameters(programId, test.name, minValue, maxValue);
        }

        await Swal.fire({
            icon: 'success',
            title: 'Éxito',
            text: 'Programa y parámetros creados exitosamente'
        });

        const modal = bootstrap.Modal.getInstance(document.getElementById('CreateParameterModal'));
        modal.hide();

        location.reload();

    } catch (error) {
        console.error("Error:", error);
        await Swal.fire({
            icon: 'error',
            title: 'Error',
            text: error.message
        });
    }
});

//delete logic
document.getElementById("DeleteParameters").addEventListener("click", async function () {
    const program = document.getElementById("ProgramSelectDelete").value;

    if (!program) {
        Swal.fire({
            icon: 'warning',
            title: 'Programa no seleccionado',
            text: 'Por favor selecciona un programa antes de eliminar.'
        });
        return;
    }

    try {
        const programId = await api.GetProgramId(program);

        if (!programId) {
            Swal.fire({
                icon: 'error',
                title: 'ID no encontrado',
                text: 'No se pudo encontrar el ID del programa seleccionado.'
            });
            return;
        }

        const confirm = await Swal.fire({
            icon: 'question',
            title: '¿Estás seguro?',
            text: `Esto eliminará el programa "${program}" y todos sus parámetros.`,
            showCancelButton: true,
            confirmButtonText: 'Sí, eliminar',
            cancelButtonText: 'Cancelar'
        });

        if (!confirm.isConfirmed) return;

        const result = await api.DeleteProgram(programId);
        console.log("Resultado de eliminación:", result);

        Swal.fire({
            icon: 'success',
            title: 'Eliminado',
            text: 'El programa fue eliminado correctamente.'
        });

        setTimeout(() => {
            location.reload();
        }, 1700); 

    } catch (error) {
        console.error("Error al eliminar:", error);

        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Ocurrió un error al intentar eliminar el programa.'
        });
    }

});

//rename logic
document.getElementById("RenameParameters").addEventListener("click", async function () {
    const currentProgram = document.getElementById("ProgramSelectRename").value;
    const newProgramName = document.getElementById("NewProgramName").value.trim().toUpperCase();

    if (!currentProgram) {
        Swal.fire({
            icon: 'warning',
            title: 'Programa no seleccionado',
            text: 'Por favor selecciona un programa antes de renombrar.'
        });
        return;
    }

    if (!newProgramName) {
        Swal.fire({
            icon: 'warning',
            title: 'Nombre vacío',
            text: 'Por favor ingresa un nuevo nombre para el programa.'
        });
        return;
    }

    try {
        const programId = await api.GetProgramId(currentProgram);

        if (!programId) {
            Swal.fire({
                icon: 'error',
                title: 'ID no encontrado',
                text: 'No se pudo encontrar el ID del programa seleccionado.'
            });
            return;
        }

        const confirm = await Swal.fire({
            icon: 'question',
            title: '¿Estás seguro?',
            text: `Esto cambiará el nombre del programa "${currentProgram}" a "${newProgramName}".`,
            showCancelButton: true,
            confirmButtonText: 'Sí, renombrar',
            cancelButtonText: 'Cancelar'
        });

        if (!confirm.isConfirmed) return;

        const result = await api.RenameProgram(programId, newProgramName);
        console.log("Resultado de renombrar:", result);

        Swal.fire({
            icon: 'success',
            title: 'Renombrado',
            text: result.message || 'El programa fue renombrado correctamente.'
        });

        setTimeout(() => {
            location.reload();
        }, 1700);

    } catch (error) {
        console.error("Error al renombrar:", error);

        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Ocurrió un error al intentar renombrar el programa.'
        });
    }
});



