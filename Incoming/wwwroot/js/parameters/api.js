export const api = {
    // ViewParameter Controller
    GetParameters: async (Program) => {
        const response = await fetch(`/ViewParameter/GetParameters?program=${encodeURIComponent(Program)}`);
        return await response.json();
    },
    GetProgramId: async (programName) => {
        const response = await fetch(`/ViewParameter/GetProgramId?programName=${encodeURIComponent(programName)}`);
        return await response.json();
    },
  


    UpdateParameters: async (updatedParams) => {
        const response = await fetch(`/ViewParameter/UpdateParameters`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(updatedParams)
        });

        return await response.json(); // O simplemente `return response.ok;`
    },

    InsertProgram: async (Program) => {
        const response = await fetch(`/InspectionPrograms/InsertInspectionProgram`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(Program)
        });

        // Verifica el tipo de contenido
        const contentType = response.headers.get("Content-Type") || "";

        if (contentType.includes("application/json")) {
            return await response.json();
        } else {
            return await response.text(); // devuelve el texto si no es JSON
        }
    },
    InsertTestParameters: async (programId, test, min, max) => {
        const parameter = {
            ProgramId: programId,
            Test: test,
            Minimum: min,
            Maximum: max
        };

        const response = await fetch(`/ViewParameter/InsertParameters`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(parameter)
        });

        if (!response.ok) {
            const error = await response.text();
            throw new Error(`Error al insertar parámetro: ${error}`);
        }

        return await response.json();
    },
    DeleteProgram: async (id) => {
        const response = await fetch(`/ViewParameter/DeleteProgram?programId=${id}`, {
            method: "DELETE"
        });

        // Devuelve texto plano si no hay contenido
        const contentType = response.headers.get("Content-Type") || "";

        if (contentType.includes("application/json")) {
            return await response.json();
        } else {
            return { message: await response.text() };
        }
    },

    RenameProgram: async (programId, newName) => {
        const response = await fetch(`/ViewParameter/RenameProgram?programId=${programId}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(newName)
        });

        const contentType = response.headers.get("Content-Type") || "";

        if (contentType.includes("application/json")) {
            return await response.json();
        } else {
            return { message: await response.text() };
        }
    }


};