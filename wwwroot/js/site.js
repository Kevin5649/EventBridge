// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring static web assets.

// =========================================
// EventBridge Dashboard Sidebar Toggle
// =========================================

document.addEventListener("DOMContentLoaded", function () {

    // =========================================
    // MULTI-TAB LOGOUT SYNCHRONIZATION
    // =========================================

    const logoutForm = document.getElementById("logoutForm");

    if (logoutForm) {

        logoutForm.addEventListener("submit", function () {

            localStorage.setItem(
                "EventBridgeLogout",
                Date.now().toString()
            );

        });

    }

    //window.addEventListener("storage", function (event) {

    //    if (event.key === "EventBridgeLogout") {

    //        window.location.replace("/Account/Login");

    //    }

    //});

    //window.addEventListener("storage", function (event) {

    //    if (event.key === "EventBridgeLogout") {

    //        window.location.replace(
    //            "/Account/Login?logout=" + Date.now()
    //        );

    //    }

    //});

    window.addEventListener("storage", function (event) {

        if (event.key === "EventBridgeLogout") {


            window.location.replace("/Account/Login");

        }

    });


    // =========================================
    // SIDEBAR TOGGLE
    // =========================================

    const sidebar = document.getElementById("customerSidebar");
    const toggleButton = document.getElementById("sidebarToggle");

    if (sidebar && toggleButton) {

        toggleButton.addEventListener("click", function () {

            sidebar.classList.toggle("open");

        });

    }


    // =========================================
    // PASSWORD SHOW / HIDE
    // Registration + Login
    // + Change Password
    // + Reset Password
    // =========================================

    const passwordInput = document.getElementById("Password");
    const confirmPasswordInput =
        document.getElementById("ConfirmPassword");

    const currentPasswordInput =
        document.getElementById("CurrentPassword");

    const newPasswordInput =
        document.getElementById("NewPassword");


    function createPasswordToggle(input) {

        if (!input) {
            return;
        }

        const wrapper = document.createElement("div");

        wrapper.className = "password-input-wrapper";

        input.parentNode.insertBefore(wrapper, input);
        wrapper.appendChild(input);

        const button = document.createElement("button");

        button.type = "button";
        button.className = "password-toggle";
        button.setAttribute("aria-label", "Show password");

        button.innerHTML =
            '<i class="fa-regular fa-eye"></i>';

        wrapper.appendChild(button);

        button.addEventListener("click", function () {

            if (input.type === "password") {

                input.type = "text";

                button.innerHTML =
                    '<i class="fa-regular fa-eye-slash"></i>';

                button.setAttribute(
                    "aria-label",
                    "Hide password"
                );

            }
            else {

                input.type = "password";

                button.innerHTML =
                    '<i class="fa-regular fa-eye"></i>';

                button.setAttribute(
                    "aria-label",
                    "Show password"
                );

            }

        });

    }


    createPasswordToggle(passwordInput);
    createPasswordToggle(confirmPasswordInput);

    createPasswordToggle(currentPasswordInput);
    createPasswordToggle(newPasswordInput);


    // =========================================
    // FORM DETECTION
    // =========================================

    const registerForm = document.querySelector(
        'form[action*="RegisterCustomer"], form[action*="RegisterPlanner"]'
    );

    const changePasswordForm = document.querySelector(
        'form[action*="ChangePassword"]'
    );

    const resetPasswordForm = document.querySelector(
        'form[action*="ResetPassword"]'
    );


    // =========================================
    // REGISTRATION VALIDATION
    // =========================================

    if (registerForm) {
        // =========================================
        // PHONE + COUNTRY CODE
        // =========================================

        const phoneInput = document.getElementById("Phone");

        const countrySelector =
            document.getElementById("customerCountrySelector") ||
            document.getElementById("plannerCountrySelector");

        const countryButton =
            document.getElementById("customerCountryButton") ||
            document.getElementById("plannerCountryButton");

        const countrySearch =
            document.getElementById("customerCountrySearch") ||
            document.getElementById("plannerCountrySearch");

        const countryList =
            document.getElementById("customerCountryList") ||
            document.getElementById("plannerCountryList");

        const countryCodeInput =
            document.getElementById("CustomerCountryCode") ||
            document.getElementById("PlannerCountryCode");

        const selectedCountryFlag =
            countryButton?.querySelector(".selected-country-flag");

        const selectedCountryCode =
            countryButton?.querySelector(".selected-country-code");

        if (
            phoneInput &&
            countrySelector &&
            countryButton &&
            countrySearch &&
            countryList &&
            countryCodeInput
        ) {

            const countries = [
                { name: "India", code: "+91", flag: "🇮🇳" },
                { name: "United States", code: "+1", flag: "🇺🇸" },
                { name: "Canada", code: "+1", flag: "🇨🇦" },
                { name: "United Kingdom", code: "+44", flag: "🇬🇧" },
                { name: "Australia", code: "+61", flag: "🇦🇺" },
                { name: "United Arab Emirates", code: "+971", flag: "🇦🇪" },
                { name: "Saudi Arabia", code: "+966", flag: "🇸🇦" },
                { name: "Singapore", code: "+65", flag: "🇸🇬" },
                { name: "Malaysia", code: "+60", flag: "🇲🇾" },
                { name: "Germany", code: "+49", flag: "🇩🇪" },
                { name: "France", code: "+33", flag: "🇫🇷" },
                { name: "Italy", code: "+39", flag: "🇮🇹" },
                { name: "Spain", code: "+34", flag: "🇪🇸" },
                { name: "Japan", code: "+81", flag: "🇯🇵" },
                { name: "China", code: "+86", flag: "🇨🇳" },
                { name: "New Zealand", code: "+64", flag: "🇳🇿" },
                { name: "South Africa", code: "+27", flag: "🇿🇦" },
                { name: "Brazil", code: "+55", flag: "🇧🇷" }
            ];

            countryCodeInput.value =
                countryCodeInput.value || "+91";

            function renderCountries(searchTerm = "") {

                const search =
                    searchTerm.trim().toLowerCase();

                const filteredCountries =
                    countries.filter(function (country) {

                        return (
                            country.name.toLowerCase().includes(search) ||
                            country.code.includes(search)
                        );

                    });

                countryList.innerHTML = "";

                if (filteredCountries.length === 0) {

                    countryList.innerHTML =
                        '<div class="country-no-results">No country found</div>';

                    return;
                }

                filteredCountries.forEach(function (country) {

                    const option =
                        document.createElement("button");

                    option.type = "button";
                    option.className = "country-option";

                    option.innerHTML = `
                <span class="country-option-flag">
                    ${country.flag}
                </span>

                <span class="country-option-name">
                    ${country.name}
                </span>

                <span class="country-option-code">
                    ${country.code}
                </span>
            `;

                    option.addEventListener("click", function () {

                        selectedCountryFlag.textContent =
                            country.flag;

                        selectedCountryCode.textContent =
                            country.code;

                        countryCodeInput.value =
                            country.code;

                        countrySearch.value = "";

                        countrySelector.classList.remove("open");

                        renderCountries();

                        phoneInput.focus();
                    });

                    countryList.appendChild(option);
                });
            }

            countryButton.addEventListener("click", function () {

                countrySelector.classList.toggle("open");

                if (countrySelector.classList.contains("open")) {

                    countrySearch.focus();

                    renderCountries();
                }

            });

            countrySearch.addEventListener("input", function () {

                renderCountries(this.value);

            });

            document.addEventListener("click", function (event) {

                if (!countrySelector.contains(event.target)) {

                    countrySelector.classList.remove("open");

                }

            });

            const phoneError =
                document.createElement("span");

            phoneError.className =
                "custom-phone-error";

            phoneError.textContent =
                "Phone number cannot start with 0.";

            phoneInput.parentNode.parentNode
                .appendChild(phoneError);

            phoneError.style.display = "none";


            phoneInput.addEventListener("input", function () {

                this.value =
                    this.value.replace(/\D/g, "");

                if (this.value.length > 10) {

                    this.value =
                        this.value.substring(0, 10);
                }

                if (
                    this.value.length > 0 &&
                    this.value.startsWith("0")
                ) {

                    phoneError.textContent =
                        "Phone number cannot start with 0.";

                    phoneError.style.display =
                        "block";

                }
                else {

                    phoneError.style.display =
                        "none";

                }

            });


            

            renderCountries();
        }

        // =========================================
        // REGISTRATION EMAIL VALIDATION
        // =========================================

        const emailInput =
            document.getElementById("Email");

        if (emailInput) {

            const emailError =
                emailInput.parentNode.querySelector(
                    ".text-danger"
                );

            emailInput.addEventListener(
                "input",
                function () {

                    const email =
                        this.value.trim();

                    const emailValid =
                        /^[^@\s]+@(gmail\.com|outlook\.com|hotmail\.com|yahoo\.com|icloud\.com)$/i
                            .test(email);

                    if (
                        email.length > 0 &&
                        !emailValid
                    ) {

                        if (emailError) {

                            emailError.textContent =
                                "Please use a Gmail, Outlook, Hotmail, Yahoo, or iCloud email address.";

                        }

                    }
                    else {

                        if (emailError) {

                            emailError.textContent =
                                "";

                        }

                    }

                }
            );

        }
        


        // =========================================
        // REGISTRATION PASSWORD VALIDATION
        // =========================================

        if (passwordInput) {

            const passwordError =
                document.createElement("span");

            passwordError.className =
                "custom-password-error";

            passwordError.textContent =
                "Password must contain at least 8 characters, one uppercase letter, one number and one special character.";

            passwordInput.parentNode.parentNode
                .appendChild(passwordError);


            passwordInput.addEventListener(
                "input",
                function () {

                    const password = this.value;

                    const hasLength =
                        password.length >= 8;

                    const hasUppercase =
                        /[A-Z]/.test(password);

                    const hasNumber =
                        /[0-9]/.test(password);

                    const hasSpecial =
                        /[^A-Za-z0-9]/.test(password);


                    if (
                        password.length > 0 &&
                        (
                            !hasLength ||
                            !hasUppercase ||
                            !hasNumber ||
                            !hasSpecial
                        )
                    ) {

                        passwordError.style.display =
                            "block";

                    }
                    else {

                        passwordError.style.display =
                            "none";

                    }

                }
            );

        }


        // =========================================
        // REGISTRATION CONFIRM PASSWORD
        // =========================================

        if (
            confirmPasswordInput &&
            passwordInput
        ) {

            const confirmError =
                document.createElement("span");

            confirmError.className =
                "custom-confirm-error";

            confirmError.textContent =
                "Passwords do not match.";

            confirmPasswordInput.parentNode.parentNode
                .appendChild(confirmError);


            confirmPasswordInput.addEventListener(
                "input",
                function () {

                    if (
                        this.value.length > 0 &&
                        this.value !== passwordInput.value
                    ) {

                        confirmError.style.display =
                            "block";

                    }
                    else {

                        confirmError.style.display =
                            "none";

                    }

                }
            );

        }


        // =========================================
        // PREVENT INVALID REGISTRATION SUBMISSION
        // =========================================

        registerForm.addEventListener(
            "submit",
            function (event) {

                let valid = true;


                // Password
                if (passwordInput) {

                    const password =
                        passwordInput.value;

                    const passwordValid =
                        password.length >= 8 &&
                        /[A-Z]/.test(password) &&
                        /[0-9]/.test(password) &&
                        /[^A-Za-z0-9]/.test(password);

                    if (!passwordValid) {

                        valid = false;

                    }

                }


                // Confirm password
                if (
                    confirmPasswordInput &&
                    passwordInput &&
                    confirmPasswordInput.value !==
                    passwordInput.value
                ) {

                    valid = false;

                }


                // Phone
                if (phoneInput) {

                    const phone =
                        phoneInput.value;

                    if (
                        !/^[1-9][0-9]{9}$/.test(phone) ||
                        phone === "0000000000"
                    ) {

                        valid = false;

                        const phoneError =
                            document.querySelector(
                                ".custom-phone-error"
                            );

                        if (phoneError) {

                            phoneError.textContent =
                                "Please enter a valid phone number.";

                            phoneError.style.display =
                                "block";

                        }

                    }

                }


                if (!valid) {

                    event.preventDefault();

                }

            }
        );

    }


    // =========================================
    // CHANGE PASSWORD VALIDATION
    // =========================================

    if (changePasswordForm) {

        const changeNewPassword =
            document.getElementById("NewPassword");

        const changeConfirmPassword =
            document.getElementById("ConfirmPassword");


        // -----------------------------------------
        // NEW PASSWORD LIVE VALIDATION
        // -----------------------------------------

        if (changeNewPassword) {

            const passwordError =
                document.createElement("span");

            passwordError.className =
                "custom-password-error";

            passwordError.textContent =
                "Password must contain at least 8 characters, one uppercase letter, one number and one special character.";

            changeNewPassword.parentNode.parentNode
                .appendChild(passwordError);


            changeNewPassword.addEventListener(
                "input",
                function () {

                    const password =
                        this.value;

                    const passwordValid =
                        password.length >= 8 &&
                        /[A-Z]/.test(password) &&
                        /[0-9]/.test(password) &&
                        /[^A-Za-z0-9]/.test(password);


                    if (
                        password.length > 0 &&
                        !passwordValid
                    ) {

                        passwordError.style.display =
                            "block";

                    }
                    else {

                        passwordError.style.display =
                            "none";

                    }


                    // Re-check confirmation
                    if (
                        changeConfirmPassword &&
                        changeConfirmPassword.value.length > 0
                    ) {

                        const confirmError =
                            document.querySelector(
                                ".custom-confirm-error"
                            );

                        if (confirmError) {

                            if (
                                changeConfirmPassword.value !==
                                changeNewPassword.value
                            ) {

                                confirmError.style.display =
                                    "block";

                            }
                            else {

                                confirmError.style.display =
                                    "none";

                            }

                        }

                    }

                }
            );

        }


        // -----------------------------------------
        // CONFIRM PASSWORD LIVE VALIDATION
        // -----------------------------------------

        if (
            changeConfirmPassword &&
            changeNewPassword
        ) {

            const confirmError =
                document.createElement("span");

            confirmError.className =
                "custom-confirm-error";

            confirmError.textContent =
                "Passwords do not match.";

            changeConfirmPassword.parentNode.parentNode
                .appendChild(confirmError);


            changeConfirmPassword.addEventListener(
                "input",
                function () {

                    if (
                        this.value.length > 0 &&
                        this.value !==
                        changeNewPassword.value
                    ) {

                        confirmError.style.display =
                            "block";

                    }
                    else {

                        confirmError.style.display =
                            "none";

                    }

                }
            );

        }


        // -----------------------------------------
        // PREVENT INVALID CHANGE PASSWORD
        // -----------------------------------------

        changePasswordForm.addEventListener(
            "submit",
            function (event) {

                let valid = true;


                if (changeNewPassword) {

                    const password =
                        changeNewPassword.value;

                    const passwordValid =
                        password.length >= 8 &&
                        /[A-Z]/.test(password) &&
                        /[0-9]/.test(password) &&
                        /[^A-Za-z0-9]/.test(password);


                    if (!passwordValid) {

                        valid = false;

                    }

                }


                if (
                    changeConfirmPassword &&
                    changeNewPassword &&
                    changeConfirmPassword.value !==
                    changeNewPassword.value
                ) {

                    valid = false;

                }


                if (!valid) {

                    event.preventDefault();

                }

            }
        );

    }


    // =========================================
    // RESET PASSWORD VALIDATION
    // =========================================

    if (resetPasswordForm) {

        const resetPasswordInput =
            document.getElementById("Password");

        const resetConfirmPassword =
            document.getElementById("ConfirmPassword");


        // -----------------------------------------
        // NEW PASSWORD LIVE VALIDATION
        // -----------------------------------------

        if (resetPasswordInput) {

            const passwordError =
                document.createElement("span");

            passwordError.className =
                "custom-password-error";

            passwordError.textContent =
                "Password must contain at least 8 characters, one uppercase letter, one number and one special character.";

            resetPasswordInput.parentNode.parentNode
                .appendChild(passwordError);


            resetPasswordInput.addEventListener(
                "input",
                function () {

                    const password =
                        this.value;

                    const passwordValid =
                        password.length >= 8 &&
                        /[A-Z]/.test(password) &&
                        /[0-9]/.test(password) &&
                        /[^A-Za-z0-9]/.test(password);


                    if (
                        password.length > 0 &&
                        !passwordValid
                    ) {

                        passwordError.style.display =
                            "block";

                    }
                    else {

                        passwordError.style.display =
                            "none";

                    }


                    // Re-check confirmation
                    if (
                        resetConfirmPassword &&
                        resetConfirmPassword.value.length > 0
                    ) {

                        const confirmError =
                            document.querySelector(
                                ".custom-confirm-error"
                            );

                        if (confirmError) {

                            if (
                                resetConfirmPassword.value !==
                                resetPasswordInput.value
                            ) {

                                confirmError.style.display =
                                    "block";

                            }
                            else {

                                confirmError.style.display =
                                    "none";

                            }

                        }

                    }

                }
            );

        }


        // -----------------------------------------
        // CONFIRM PASSWORD LIVE VALIDATION
        // -----------------------------------------

        if (
            resetConfirmPassword &&
            resetPasswordInput
        ) {

            const confirmError =
                document.createElement("span");

            confirmError.className =
                "custom-confirm-error";

            confirmError.textContent =
                "Passwords do not match.";

            resetConfirmPassword.parentNode.parentNode
                .appendChild(confirmError);


            resetConfirmPassword.addEventListener(
                "input",
                function () {

                    if (
                        this.value.length > 0 &&
                        this.value !==
                        resetPasswordInput.value
                    ) {

                        confirmError.style.display =
                            "block";

                    }
                    else {

                        confirmError.style.display =
                            "none";

                    }

                }
            );

        }


        // -----------------------------------------
        // PREVENT INVALID RESET PASSWORD
        // -----------------------------------------

        resetPasswordForm.addEventListener(
            "submit",
            function (event) {

                let valid = true;


                if (resetPasswordInput) {

                    const password =
                        resetPasswordInput.value;

                    const passwordValid =
                        password.length >= 8 &&
                        /[A-Z]/.test(password) &&
                        /[0-9]/.test(password) &&
                        /[^A-Za-z0-9]/.test(password);


                    if (!passwordValid) {

                        valid = false;

                    }

                }


                if (
                    resetConfirmPassword &&
                    resetPasswordInput &&
                    resetConfirmPassword.value !==
                    resetPasswordInput.value
                ) {

                    valid = false;

                }


                if (!valid) {

                    event.preventDefault();

                }

            }
        );

    }




    // =========================================
    // NUMBER INPUT BEHAVIOR
    // Prevent accidental value changes from
    // mouse wheel / touchpad and arrow keys
    // =========================================

    const numberInputs =
        document.querySelectorAll(
            '.enquiry-form-control[type="number"], ' +
            '.quotation-form-control[type="number"]'
        );

    numberInputs.forEach(function (input) {

        // Clear the initial 0 when the field is loaded
        if (input.value === "0") {
            input.value = "";
        }


        input.addEventListener(
            "wheel",
            function () {

                if (document.activeElement === this) {
                    this.blur();
                }

            },
            { passive: true }
        );


        input.addEventListener(
            "keydown",
            function (event) {

                if (
                    event.key === "ArrowUp" ||
                    event.key === "ArrowDown"
                ) {

                    event.preventDefault();

                }

            }
        );

    });

});