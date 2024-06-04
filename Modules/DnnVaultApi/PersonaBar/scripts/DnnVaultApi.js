'use strict';

define(['jquery',
    'main/config'
],
    function ($, cf) {
        var utility;
        var config = cf.init();

        var initializationMethod = function (wrapper, util, params, callback) {
            //console.log(wrapper);
            //console.log(util);
            //console.log(params);
            //console.log(callback);

            var baseRoute = 'DnnVaultApi';
            var sf = window.dnn.utility.sf;

            // Manage local secrets configuration
            $('#local-certificate-save-config').prop('disabled', true);

            $(document).ready(function () {
                // Get the settings for each host
                // Build the route to the controller method
                var route = `${baseRoute}/DnnVaultApi/GetDnnVaultSettings`;

                // Call the server 
                sf.get(route, {}, function (response) {
                    if (response) {
                        // Loop through the properties of the response object
                        for (var setting in response) {
                            // Set the value of the input with the name property value = setting to the value of the setting
                            $(`input[name="${setting}"]`).val(response[setting]);
                        }
                    } else {
                        console.log('Enable App Secrets Encryption');
                    }
                });
            });

            /**
             * This method handles both the testing and saving of the settings for each host
             * by using the data-host and data-action attributes of the button clicked
             */
            $('.vault-settings-button').click(function (event) {
                event.preventDefault();

                // Update the UI
                $('#test-failure').hide();
                $('#save-in-progress').hide();
                $('#save-success').hide();
                $('#test-in-progess').show();

                // Get the host from the clicked button. The host value aligns with the SecretsRepository.SecretHost enum.
                const host = event.target.getAttribute('data-host');

                // Get the action from the clicked button. The action value aligns with the DnnVaultApiController method name.
                const action = event.target.getAttribute('data-action');

                // Get all the html inputs marked with data-host = host
                const settingsElements = $(`input[data-host="${host}"]`);

                // loop through the settingsElements and build the settings object.
                const settings = {};
                settingsElements.each(function () {
                    settings[this.name] = this.value;
                });

                // Build the route to the controller method
                var route = `${baseRoute}/DnnVaultApi/ReceiveDnnVaultSettings`;

                // Build the payload object to be sent to the server
                var payload = {
                    Host: host,
                    Action: action,
                    Settings: settings
                };

                // Build the success callback
                var successResult = function (response) {
                    if (response[host]) {
                        $('#test-in-progess').hide();
                        $('#test-success').show();
                        $('#local-certificate-save-config').prop('disabled', false);
                    } else {
                        $('#test-in-progess').hide();
                        $('#test-failure').show();
                    }
                };

                // Call the server with the payload
                sf.post(route, payload, successResult);
            });

            $('#toggle-connection-string-encryption').click(function (event) {
                event.preventDefault();
                
                // Build the route to the controller method
                var route = `${baseRoute}/DnnVaultApi/ToggleConnectionStringEncryption`;

                // Call the server 
                sf.get(route, function (response) {
                    if (response) {
                        console.log('Disable Connection String Encryption');
                    } else {
                        console.log('Enable Connection String Encryption');
                    }
                });
            });

            $('#toggle-secrets-encryption').click(function (event) {
                event.preventDefault();
                
                // Build the route to the controller method
                var route = `${baseRoute}/DnnVaultApi/ToggleAppSecretsEncryption`;

                // Call the server 
                sf.get(route, function (response) {
                    if (response) {
                        console.log('Disable App Secrets Encryption');
                    } else {
                        console.log('Enable App Secrets Encryption');
                    }
                });
            });
        }

        return {
            init: initializationMethod,

            initMobile: function (wrapper, utility, params, callback) {
            },

            load: function (params, callback) {
            },

            loadMobile: function (params, callback) {
            }
        };
    });


