'use strict';

define(['jquery',
    'main/config'
],
    function ($, cf) {
        var utility;
        var config = cf.init();

        return {
            init: function (wrapper, util, params, callback) {
                console.log(wrapper);
                console.log(util);
                console.log(params);
                console.log(callback);
            //    window.initDnnVaultApi = function initializeDnnVaultApi() {
            //        // I don't know how or where to consume these values
            //        return {
            //            utility: utility,
            //            siteRoot: config.siteRoot,
            //            settings: params.settings,
            //            moduleName: 'DnnVaultApi',
            //            identifier: params.identifier
            //        };
            //    };

            //    // If we need to load a script bundle, we can use the utility.loadBundleScript method.
            //    // utility.loadBundleScript('modules/dnn.sitesettings/scripts/bundles/site-settings-bundle.js');

            //    if (typeof callback === "function") {
            //        callback();
            //    }
            },

            initMobile: function (wrapper, utility, params, callback) {
            },

            load: function (params, callback) {
            },

            loadMobile: function (params, callback) {
            }
        };
    });


