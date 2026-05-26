
// Realiza las peticiones AJAX para la interacción con el backend.
export const api = {
    // ViewParameter Controller
    getPartDetails: async (partNumber) => {
        const response = await fetch(`/ViewParameter/Details?partNumber=${encodeURIComponent(partNumber)}`);
        return await response.json();
    },

    // Defects Controller
    getDefects: async () => {
        const response = await fetch('/Defects/Details');
        return await response.json();
    },

    // Rejections Controller
    insertRejectionData: async (data) => {
        try {
            const response = await fetch('/Rejections/InsertData', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (!response.ok) {
                // Si es un conflicto (409), verificar el tipo
                if (response.status === 409) {
                    // Si tiene defectos previos, lanzar error especial con datos completos
                    if (result.hasDefects) {
                        const error = new Error(result.message || 'El volante ya fue escaneado con defectos');
                        error.conflictData = result; // Adjuntar toda la información del conflicto
                        error.conflictType = 'rejection';
                        throw error;
                    }
                }

                let errorMessage;
                switch (response.status) {
                    case 400:
                        errorMessage = result.message || 'Datos inválidos';
                        break;
                    case 409:
                        errorMessage = result.message || 'El volante ya fue escaneado';
                        break;
                    default:
                        errorMessage = result.message || `Error ${response.status}: ${response.statusText}`;
                }
                throw new Error(errorMessage);
            }

            return result;

        } catch (error) {
            console.error("Error en la inserción:", error);
            throw error;
        }
    },

    getRejectionCounts: async () => {
        const response = await fetch('/Rejections/GetRejectionCounts');
        return await response.json();
    },

    insertAcceptedData: async (data) => {
        try {
            const response = await fetch('/Rejections/InsertAcceptedData', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (!response.ok) {
                // Si es un conflicto (409), verificar el tipo
                if (response.status === 409) {
                    // Si tiene defectos previos, lanzar error especial con datos completos
                    if (result.hasDefects) {
                        const error = new Error(result.message || 'El volante ya fue escaneado con defectos');
                        error.conflictData = result; // Adjuntar toda la información del conflicto
                        error.conflictType = 'rejection';
                        throw error;
                    }
                    // Si ya está en aceptados (hasDefects: false), lanzar error especial
                    if (result.hasDefects === false && result.acceptedDate) {
                        const error = new Error(result.message || 'El volante ya fue escaneado sin defectos');
                        error.conflictData = result;
                        error.conflictType = 'accepted';
                        throw error;
                    }
                }

                let errorMessage;
                switch (response.status) {
                    case 400:
                        errorMessage = result.message || 'Datos inválidos';
                        break;
                    case 409:
                        errorMessage = result.message || 'El volante ya fue escaneado';
                        break;
                    default:
                        errorMessage = result.message || `Error ${response.status}: ${response.statusText}`;
                }
                throw new Error(errorMessage);
            }

            return result;

        } catch (error) {
            console.error("Error en la inserción:", error);
            throw error;
        }
    },

    convertRejectionToAccepted: async (data) => {
        try {
            const response = await fetch('/Rejections/ConvertRejectionToAccepted', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (!response.ok) {
                let errorMessage;
                switch (response.status) {
                    case 400:
                        errorMessage = result.message || 'Datos inválidos';
                        break;
                    case 404:
                        errorMessage = result.message || 'No se encontró el volante en rechazos';
                        break;
                    default:
                        errorMessage = result.message || `Error ${response.status}: ${response.statusText}`;
                }
                throw new Error(errorMessage);
            }

            return result;

        } catch (error) {
            console.error("Error en la conversión:", error);
            throw error;
        }
    },

    // Finishedgood Controller
    insertSteeringWheelData: async (data) => {
        try {
            const response = await fetch('/Finishedgoods/InsertData', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (!response.ok) {
                let errorMessage;
                switch (response.status) {
                    case 400:
                        errorMessage = result.message || 'Datos inválidos';
                        break;
                    case 409:
                        errorMessage = result.message || 'El volante ya fue escaneado';
                        break;
                    default:
                        errorMessage = result.message || `Error ${response.status}: ${response.statusText}`;
                }
                throw new Error(errorMessage);
            }

            return result;

        } catch (error) {
            console.error("Error en la inserción:", error);
             
            throw error;
        }
    },
    // Containers Controller
    insertContainerData: async (data) => {
        try {
            const response = await fetch('/Containers/InsertData', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (!response.ok) {
                let errorMessage;
                switch (response.status) {
                    case 400:
                        errorMessage = result.message || 'Datos inválidos';
                        break;
                    case 409:
                        errorMessage = result.message || 'Ya existe un contenedor en curso, terminolo de Inspeccionar';
                        break;
                    default:
                        errorMessage = result.message || `Error ${response.status}: ${response.statusText}`;
                }
                throw new Error(errorMessage);
            }

            return result;

        } catch (error) {
            console.error("Error en la inserción:", error);
            throw error;
        }
    },

    getContainerCounts: async (program) => {
        try {
            const response = await fetch(`/Containers/GetProgramCounts?program=${encodeURIComponent(program)}`);
            if (!response.ok) throw new Error(`HTTP error: ${response.status}`);
            const result = await response.json();
            return {
                Quantity: result.quantity || 0,
                PartialCount: result.partialCount || 0
            };
        } catch (error) {
            console.error("Error fetching counts:", error);
            return { Quantity: 0, PartialCount: 0 };
        }
    },

    getContainerQuantity: async (program) => {
        try {
            const response = await fetch(
                `/Containers/GetQuantity?program=${encodeURIComponent(program)}`
            );
            if (!response.ok) throw new Error(`HTTP error: ${response.status}`);
            const result = await response.json();
            return { Quantity: result.quantity ?? 0 };
        } catch (error) {
            console.error("Error fetching quantity:", error);
            return { Quantity: 0 };
        }
    },

    getContainerStatus: async (program) => {
        try {
            const response = await fetch(
                `/Containers/GetProgramStatus?program=${encodeURIComponent(program)}`
            );
            if (!response.ok) throw new Error(`HTTP error: ${response.status}`);
            const result = await response.json();
            return { Status: result.status || 'UNKNOWN' };
        } catch (error) {
            console.error("Error fetching status:", error);
            return { Status: 'ERROR' };
        }
    },

    updateContainerQuantity: async (program, quantity) => {
        try {
            const response = await fetch(`/Containers/UpdateContainerQuantity?program=${encodeURIComponent(program)}&quantity=${quantity}`, {
                method: 'POST'
            });

            if (!response.ok) {
                throw new Error(`HTTP error: ${response.status}`);
            }

            const result = await response.json();
            return result;
        } catch (error) {
            console.error("Error updating container quantity:", error);
            return { error: 'ERROR' };
        }
    },

    UpdatePartialCount: async (program) => {
        try {
            const response = await fetch(`/Containers/UpdatePartialCount?program=${encodeURIComponent(program)}`, {
                method: 'POST'
            });

            const result = await response.json();

            if (!response.ok) {
                let errorMessage;
                switch (response.status) {
                    case 400:
                        errorMessage = result.message || 'Datos inválidos';
                        break;
                    case 409:
                        errorMessage = result.message || 'Ya existe un contenedor en curso, terminolo de Inspeccionar';
                        break;
                    default:
                        errorMessage = result.message || `Error ${response.status}: ${response.statusText}`;
                }
                throw new Error(errorMessage);
            }

            return result;

        } catch (error) {
            console.error("Error en la inserción:", error);
            throw error;
        }
    },

    getDefectData: async (id) => {
        try {
            const response = await fetch(`/Rejections/GetDefectData?id=${id}`);
            const result = await response.json();

            if (!response.ok) {
                let errorMessage;
                switch (response.status) {
                    case 404:
                        errorMessage = result.message || 'Defecto no encontrado';
                        break;
                    default:
                        errorMessage = result.message || `Error ${response.status}: ${response.statusText}`;
                }
                throw new Error(errorMessage);
            }

            return result;

        } catch (error) {
            console.error("Error al obtener defecto:", error);
            throw error;
        }
    },

    updateDefect: async (data) => {
        try {
            const response = await fetch('/Rejections/UpdateDefect', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (!response.ok) {
                let errorMessage;
                switch (response.status) {
                    case 400:
                        errorMessage = result.message || 'Datos inválidos';
                        break;
                    case 404:
                        errorMessage = result.message || 'Defecto no encontrado';
                        break;
                    default:
                        errorMessage = result.message || `Error ${response.status}: ${response.statusText}`;
                }
                throw new Error(errorMessage);
            }

            return result;

        } catch (error) {
            console.error("Error al actualizar defecto:", error);
            throw error;
        }
    },


    // Shutdown Controller
    shutdownPC: async () => {
        await fetch('/Shutdown/Shutdown', { method: 'POST' });

    }

};
