
"use strict";

// Initialize or reuse the global SDK namespace
var SDK = window.SDK || {};

// Execute the IIFE with SDK as its 'this' context
(function () {

    // Unique ID used to identify the form notification
    this.notificationId = "account_welcome_notification";

    // Welcome notification message
    this.welcomeMessage =
        "Welcome! Please review the Account details before saving.";


    // Account form OnLoad event handler
    this.onLoad = function (executionContext) {

        // Get the current form context
        var formContext = executionContext.getFormContext();

        // Display an informational notification on the form
        formContext.ui.setFormNotification(
            this.welcomeMessage,
            "INFO",
            this.notificationId
        );
    };


    // Account Name OnChange event handler
    this.onAccountNameChange = function (executionContext) {

        // Get the current form context
        var formContext = executionContext.getFormContext();

        // Retrieve the Account Name column value
        var accountName = formContext.getAttribute("name").getValue();

        // Check whether Account Name contains a non-empty value
        if (accountName && accountName.trim() !== "") {

            // Clear the notification when Account Name is populated
            formContext.ui.clearFormNotification(
                this.notificationId
            );

        } else {

            // Display the notification when Account Name is empty
            formContext.ui.setFormNotification(
                this.welcomeMessage,
                "INFO",
                this.notificationId
            );
        }
    };

}).call(SDK);
