/**
 * login.js — Lógica del formulario de login de Transporte Escolar
 * - Muestra/oculta campo de ficha según rol seleccionado
 * - Toggle de visibilidad de contraseña
 * - Feedback visual al enviar
 */

(function () {
    'use strict';

    const fichaGroup = document.getElementById('fichaGroup');
    const fichaInput = document.getElementById('fichaInput');
    const roleOptions = document.querySelectorAll('.role-option input[type="radio"]');
    const toggleBtn = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('passwordInput');
    const eyeIcon = document.getElementById('eyeIcon');
    const loginForm = document.getElementById('loginForm');
    const btnLogin = document.getElementById('btnLogin');
    const btnText = btnLogin?.querySelector('.btn-text');
    const btnLoader = btnLogin?.querySelector('.btn-loader');

    /** Mostrar u ocultar el campo de ficha del autobús */
    function toggleFichaField(rolSeleccionado) {
        const esAzafata = rolSeleccionado === 'Azafata';

        if (esAzafata) {
            fichaGroup.style.display = 'block';
            fichaInput.setAttribute('required', 'required');
        } else {
            fichaGroup.style.display = 'none';
            fichaInput.removeAttribute('required');
            fichaInput.value = '';
        }
    }

    /** Registrar eventos en los radio buttons de rol */
    roleOptions.forEach(function (radio) {
        radio.addEventListener('change', function () {
            toggleFichaField(this.value);
        });
    });

    /** Si al cargar la página ya hay un rol seleccionado (p.ej. por validación fallida) */
    const rolMarcado = document.querySelector('.role-option input[type="radio"]:checked');
    if (rolMarcado) {
        toggleFichaField(rolMarcado.value);
    }

    /** Toggle mostrar/ocultar contraseña */
    if (toggleBtn && passwordInput) {
        toggleBtn.addEventListener('click', function () {
            const visible = passwordInput.type === 'text';
            passwordInput.type = visible ? 'password' : 'text';

            // Cambiar icono
            eyeIcon.innerHTML = visible
                ? `<path d="M10 12.5a2.5 2.5 0 100-5 2.5 2.5 0 000 5z"/>
           <path fill-rule="evenodd" d="M.664 10.59a1.651 1.651 0 010-1.186A10.004 10.004 0 0110 3c4.257 0 7.893 2.66 9.336 6.41.147.381.146.804 0 1.186A10.004 10.004 0 0110 17c-4.257 0-7.893-2.66-9.336-6.41zM14 10a4 4 0 11-8 0 4 4 0 018 0z" clip-rule="evenodd"/>`
                : `<path fill-rule="evenodd" d="M3.28 2.22a.75.75 0 00-1.06 1.06l14.5 14.5a.75.75 0 101.06-1.06l-1.745-1.745a10.029 10.029 0 003.3-4.38 1.651 1.651 0 000-1.185A10.004 10.004 0 009.999 3a9.956 9.956 0 00-4.744 1.194L3.28 2.22zM7.752 6.69l1.092 1.092a2.5 2.5 0 013.374 3.373l1.091 1.092a4 4 0 00-5.557-5.557z" clip-rule="evenodd"/>
           <path d="M10.748 13.93l2.523 2.523a10.003 10.003 0 01-8.945-5.589 1.651 1.651 0 010-1.185 10.004 10.004 0 012.57-3.43l1.54 1.54a4 4 0 005.312 6.101z"/>`;
        });
    }

    /** Feedback visual al enviar el formulario */
    if (loginForm && btnLogin && btnText && btnLoader) {
        loginForm.addEventListener('submit', function () {
            // Solo mostrar loader si el formulario es válido (HTML5)
            if (loginForm.checkValidity()) {
                btnText.style.display = 'none';
                btnLoader.style.display = 'flex';
                btnLogin.disabled = true;
            }
        });
    }

})();