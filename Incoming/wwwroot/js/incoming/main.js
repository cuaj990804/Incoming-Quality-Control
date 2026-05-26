// main.js - Inicialización principal
import { scanner } from './scanner.js';
import { events } from './events.js';
import { api } from './api.js';
import { ui } from './ui.js';

document.addEventListener('DOMContentLoaded', () => {
    // Carga inicial de datos
    scanner.init();
    events.init();
    api.getRejectionCounts()
        .then(ui.updateRejectionList)
        .catch(console.error);

    ui.elements.baseInput.focus();


});