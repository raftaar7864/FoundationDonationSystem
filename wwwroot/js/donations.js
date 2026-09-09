// =========================================================
// ADMIN DONATIONS PAGE
// =========================================================
// =========================================================
// GLOBAL STATES
// =========================================================
let pendingDeleteDonationId = null;
let pendingVerifyDonationId = null;
let pendingRejectDonationId = null;
// =========================================================
// OPEN DONATION DETAILS MODAL
// =========================================================
async function openDonationDetailsModal(url, title) {
    const modal =
        document.getElementById("donationDetailsModal");
    const body =
        document.getElementById("donationDetailsModalBody");
    const titleElement =
        document.getElementById("donationDetailsModalTitle");
    if (!modal || !body) {
        console.error(
            "Donation details modal elements not found."
        );
        return;
    }
    if (titleElement) {
        titleElement.textContent =
            title || "Donation Details";
    }
    body.innerHTML =
        '<div class="donation-modal-loading">' +
        '<span class="verify-button-spinner"></span>' +
        '<span>Loading...</span>' +
        '</div>';
    modal.classList.add("show");
    document.body.classList.add(
        "donation-modal-open"
    );
    try {
        const response =
            await fetch(
                url,
                {
                    method: "GET",
                    headers: {
                        "X-Requested-With":
                            "XMLHttpRequest",
                        "Accept":
                            "text/html"
                    }
                }
            );
        if (!response.ok) {
            throw new Error(
                "Unable to load donation details."
            );
        }
        const html =
            await response.text();
        body.innerHTML =
            html;
    }
    catch (error) {
        console.error(
            "Donation details error:",
            error
        );
        body.innerHTML =
            '<div class="donation-modal-error">' +
            (
                error.message ||
                "Unable to load donation details."
            ) +
            '</div>';
    }
}
// =========================================================
// CLOSE DONATION DETAILS MODAL
// =========================================================
function closeDonationDetailsModal() {
    const modal =
        document.getElementById(
            "donationDetailsModal"
        );
    if (modal) {
        modal.classList.remove("show");
    }
    document.body.classList.remove(
        "donation-modal-open"
    );
    const body =
        document.getElementById(
            "donationDetailsModalBody"
        );
    if (body) {
        body.innerHTML = "";
    }
}
// =========================================================
// GENERATE DONATION SLIP
// =========================================================
function generateDonationSlip(donationId, button) {
    if (!donationId) {
        alert("Donation ID is missing.");
        return;
    }
    if (!button) {
        return;
    }
    if (button.disabled) {
        return;
    }
    const tokenElement =
        document.querySelector(
            '#donation-details-antiforgery input[name="__RequestVerificationToken"]'
        ) ||
        document.querySelector(
            '#donationDetailsModal input[name="__RequestVerificationToken"]'
        ) ||
        document.querySelector(
            'input[name="__RequestVerificationToken"]'
        );
    if (!tokenElement) {
        alert(
            "Security token was not found. Please refresh the page."
        );
        return;
    }
    const originalText =
        button.innerHTML;
    button.disabled = true;
    button.innerHTML =
        '<span class="verify-button-spinner"></span> Generating...';
    // ---------------------------------------------------------
    // Create a completely independent POST form.
    // This avoids problems if the modal happens to be inside
    // another form on the page.
    // ---------------------------------------------------------
    const form =
        document.createElement("form");
    form.method = "POST";
    form.action =
        "/Admin/Donations/GenerateSlip";
    form.style.display = "none";
    // Donation ID
    const donationIdInput =
        document.createElement("input");
    donationIdInput.type = "hidden";
    donationIdInput.name =
        "donationId";
    donationIdInput.value =
        donationId;
    form.appendChild(
        donationIdInput
    );
    // Anti-forgery token
    const tokenInput =
        document.createElement("input");
    tokenInput.type = "hidden";
    tokenInput.name =
        "__RequestVerificationToken";
    tokenInput.value =
        tokenElement.value;
    form.appendChild(
        tokenInput
    );
    document.body.appendChild(form);
    // ---------------------------------------------------------
    // Submit normally.
    //
    // The controller redirects to ViewSlip after successful
    // generation, so the browser will navigate to the receipt.
    // ---------------------------------------------------------
    try {
        form.submit();
    }
    catch (error) {
        console.error(
            "Generate donation slip error:",
            error
        );
        document.body.removeChild(
            form
        );
        button.disabled = false;
        button.innerHTML =
            originalText;
        alert(
            "Unable to generate donation slip."
        );
    }
}
// =========================================================
// OPEN OFFLINE DONATION MODAL
// =========================================================
async function openOfflineDonationModal() {
    const modal =
        document.getElementById(
            "offlineDonationModal"
        );
    const body =
        document.getElementById(
            "offlineDonationModalBody"
        );
    if (!modal || !body) {
        console.error(
            "Offline donation modal elements not found."
        );
        return;
    }
    body.innerHTML =
        '<div class="donation-modal-loading">' +
        '<span class="verify-button-spinner"></span>' +
        '<span>Loading...</span>' +
        '</div>';
    modal.classList.add("show");
    document.body.classList.add(
        "donation-modal-open"
    );
    try {
        const response =
            await fetch(
                "/Admin/Donations/CollectOffline",
                {
                    method: "GET",
                    headers: {
                        "X-Requested-With":
                            "XMLHttpRequest",
                        "Accept":
                            "text/html"
                    }
                }
            );
        if (!response.ok) {
            throw new Error(
                "Unable to load offline donation form."
            );
        }
        const html =
            await response.text();
        body.innerHTML =
            html;
        // The offline form is loaded dynamically.
        // Initialize its JavaScript after inserting it.
        if (
            typeof window.initializeOfflineDonationForm ===
            "function"
        ) {
            window.initializeOfflineDonationForm();
        }
    }
    catch (error) {
        console.error(
            "Offline donation error:",
            error
        );
        body.innerHTML =
            '<div class="donation-modal-error">' +
            (
                error.message ||
                "Unable to load offline donation form."
            ) +
            '</div>';
    }
}
// =========================================================
// CLOSE OFFLINE DONATION MODAL
// =========================================================
function closeOfflineDonationModal() {
    const modal =
        document.getElementById(
            "offlineDonationModal"
        );
    if (modal) {
        modal.classList.remove("show");
    }
    document.body.classList.remove(
        "donation-modal-open"
    );
    const body =
        document.getElementById(
            "offlineDonationModalBody"
        );
    if (body) {
        body.innerHTML = "";
    }
}
// =========================================================
// VERIFY DONATION
// =========================================================
// ---------------------------------------------------------
// OPEN VERIFY CONFIRMATION
// ---------------------------------------------------------
function openVerifyConfirmation(id) {
    pendingVerifyDonationId = id;
    const modal =
        document.getElementById(
            "donationVerifyModal"
        );
    const button =
        document.getElementById(
            "confirmVerifyButton"
        );
    if (!modal) {
        console.error(
            "Donation verify modal not found."
        );
        return;
    }
    if (button) {
        button.disabled = false;
        button.innerHTML =
            "Verify Donation";
    }
    modal.classList.add("show");
    document.body.classList.add(
        "donation-modal-open"
    );
}
// ---------------------------------------------------------
// CLOSE VERIFY CONFIRMATION
// ---------------------------------------------------------
function closeVerifyConfirmation() {
    const modal =
        document.getElementById(
            "donationVerifyModal"
        );
    pendingVerifyDonationId = null;
    if (modal) {
        modal.classList.remove("show");
    }
    document.body.classList.remove(
        "donation-modal-open"
    );
    const button =
        document.getElementById(
            "confirmVerifyButton"
        );
    if (button) {
        button.disabled = false;
        button.innerHTML =
            "Verify Donation";
    }
}
// ---------------------------------------------------------
// CONFIRM VERIFY DONATION
// ---------------------------------------------------------
async function confirmVerifyDonation() {
    if (!pendingVerifyDonationId) {
        return;
    }
    const id =
        pendingVerifyDonationId;
    const tokenElement =
        document.querySelector(
            'input[name="__RequestVerificationToken"]'
        );
    if (!tokenElement) {
        alert(
            "Security token was not found. Please refresh the page."
        );
        return;
    }
    const button =
        document.getElementById(
            "confirmVerifyButton"
        );
    const originalText =
        button
            ? button.innerHTML
            : "Verify Donation";
    try {
        if (button) {
            button.disabled = true;
            button.innerHTML =
                '<span class="verify-button-spinner"></span> Verifying...';
        }
        const formData =
            new FormData();
        formData.append(
            "__RequestVerificationToken",
            tokenElement.value
        );
        formData.append(
            "id",
            id
        );
        const response =
            await fetch(
                "/Admin/Donations/Verify",
                {
                    method: "POST",
                    headers: {
                        "X-Requested-With":
                            "XMLHttpRequest",
                        "Accept":
                            "application/json"
                    },
                    body: formData
                }
            );
        const responseText =
            await response.text();
        let result;
        try {
            result =
                JSON.parse(
                    responseText
                );
        }
        catch {
            throw new Error(
                "Server returned an unexpected response. HTTP Status: " +
                response.status
            );
        }
        if (!response.ok) {
            throw new Error(
                result.message ||
                "Unable to verify donation."
            );
        }
        if (!result.success) {
            throw new Error(
                result.message ||
                "Unable to verify donation."
            );
        }
        closeVerifyConfirmation();
        closeDonationDetailsModal();
        window.location.reload();
    }
    catch (error) {
        console.error(
            "Verify donation error:",
            error
        );
        if (button) {
            button.disabled = false;
            button.innerHTML =
                originalText;
        }
        alert(
            error.message ||
            "Unable to verify donation."
        );
    }
}
// =========================================================
// REJECT DONATION
// =========================================================
// ---------------------------------------------------------
// OPEN REJECT CONFIRMATION
// ---------------------------------------------------------
function openRejectConfirmation(id) {
    pendingRejectDonationId = id;
    const modal =
        document.getElementById(
            "donationRejectModal"
        );
    const button =
        document.getElementById(
            "confirmRejectButton"
        );
    const remarks =
        document.getElementById(
            "rejectionRemarks"
        );
    if (!modal) {
        console.error(
            "Donation reject modal not found."
        );
        return;
    }
    if (button) {
        button.disabled = false;
        button.innerHTML =
            "Reject Donation";
    }
    if (remarks) {
        remarks.value = "";
        updateRejectionRemarksCounter();
    }
    modal.classList.add("show");
    document.body.classList.add(
        "donation-modal-open"
    );
}
// ---------------------------------------------------------
// CLOSE REJECT CONFIRMATION
// ---------------------------------------------------------
function closeRejectConfirmation() {
    const modal =
        document.getElementById(
            "donationRejectModal"
        );
    pendingRejectDonationId = null;
    if (modal) {
        modal.classList.remove("show");
    }
    document.body.classList.remove(
        "donation-modal-open"
    );
    const remarks =
        document.getElementById(
            "rejectionRemarks"
        );
    if (remarks) {
        remarks.value = "";
        updateRejectionRemarksCounter();
    }
    const button =
        document.getElementById(
            "confirmRejectButton"
        );
    if (button) {
        button.disabled = false;
        button.innerHTML =
            "Reject Donation";
    }
}
// ---------------------------------------------------------
// UPDATE REJECTION REMARKS COUNTER
// ---------------------------------------------------------
function updateRejectionRemarksCounter() {
    const remarks =
        document.getElementById(
            "rejectionRemarks"
        );
    const counter =
        document.getElementById(
            "rejectionRemarksCounter"
        );
    if (!remarks || !counter) {
        return;
    }
    counter.textContent =
        remarks.value.length;
}
// ---------------------------------------------------------
// CONFIRM REJECT DONATION
// ---------------------------------------------------------
async function confirmRejectDonation() {
    if (!pendingRejectDonationId) {
        return;
    }
    const id =
        pendingRejectDonationId;
    const remarksElement =
        document.getElementById(
            "rejectionRemarks"
        );
    if (!remarksElement) {
        alert(
            "Rejection remarks field was not found."
        );
        return;
    }
    const remarks =
        remarksElement.value.trim();
    if (!remarks) {
        alert(
            "Please enter a reason for rejecting this donation."
        );
        remarksElement.focus();
        return;
    }
    if (remarks.length > 500) {
        alert(
            "Rejection remarks cannot exceed 500 characters."
        );
        remarksElement.focus();
        return;
    }
    const tokenElement =
        document.querySelector(
            'input[name="__RequestVerificationToken"]'
        );
    if (!tokenElement) {
        alert(
            "Security token was not found. Please refresh the page."
        );
        return;
    }
    const button =
        document.getElementById(
            "confirmRejectButton"
        );
    const originalText =
        button
            ? button.innerHTML
            : "Reject Donation";
    try {
        if (button) {
            button.disabled = true;
            button.innerHTML =
                '<span class="verify-button-spinner"></span> Rejecting...';
        }
        const formData =
            new FormData();
        formData.append(
            "__RequestVerificationToken",
            tokenElement.value
        );
        formData.append(
            "id",
            id
        );
        formData.append(
            "remarks",
            remarks
        );
        const response =
            await fetch(
                "/Admin/Donations/Reject",
                {
                    method: "POST",
                    headers: {
                        "X-Requested-With":
                            "XMLHttpRequest",
                        "Accept":
                            "application/json"
                    },
                    body: formData
                }
            );
        const responseText =
            await response.text();
        let result;
        try {
            result =
                JSON.parse(
                    responseText
                );
        }
        catch {
            throw new Error(
                "Server returned an unexpected response. HTTP Status: " +
                response.status
            );
        }
        if (!response.ok) {
            throw new Error(
                result.message ||
                "Unable to reject donation."
            );
        }
        if (!result.success) {
            throw new Error(
                result.message ||
                "Unable to reject donation."
            );
        }
        closeRejectConfirmation();
        closeDonationDetailsModal();
        window.location.reload();
    }
    catch (error) {
        console.error(
            "Reject donation error:",
            error
        );
        if (button) {
            button.disabled = false;
            button.innerHTML =
                originalText;
        }
        alert(
            error.message ||
            "Unable to reject donation."
        );
    }
}
// =========================================================
// DELETE DONATION
// =========================================================
// ---------------------------------------------------------
// OPEN DELETE CONFIRMATION
// ---------------------------------------------------------
function openDeleteConfirmation(
    id,
    donationNumber
) {
    pendingDeleteDonationId = id;
    const modal =
        document.getElementById(
            "donationDeleteModal"
        );
    const numberElement =
        document.getElementById(
            "deleteDonationNumber"
        );
    const deleteButton =
        document.getElementById(
            "confirmDeleteButton"
        );
    if (!modal) {
        console.error(
            "Donation delete modal not found."
        );
        return;
    }
    if (numberElement) {
        numberElement.textContent =
            donationNumber || "";
    }
    if (deleteButton) {
        deleteButton.disabled = false;
        deleteButton.innerHTML =
            "Delete Donation";
    }
    modal.classList.add("show");
    document.body.classList.add(
        "donation-modal-open"
    );
}
// ---------------------------------------------------------
// CLOSE DELETE CONFIRMATION
// ---------------------------------------------------------
function closeDeleteConfirmation() {
    const modal =
        document.getElementById(
            "donationDeleteModal"
        );
    pendingDeleteDonationId = null;
    if (modal) {
        modal.classList.remove("show");
    }
    document.body.classList.remove(
        "donation-modal-open"
    );
    const numberElement =
        document.getElementById(
            "deleteDonationNumber"
        );
    if (numberElement) {
        numberElement.textContent = "";
    }
    const deleteButton =
        document.getElementById(
            "confirmDeleteButton"
        );
    if (deleteButton) {
        deleteButton.disabled = false;
        deleteButton.innerHTML =
            "Delete Donation";
    }
}
// ---------------------------------------------------------
// CONFIRM DELETE DONATION
// ---------------------------------------------------------
async function confirmDeleteDonation() {
    if (!pendingDeleteDonationId) {
        return;
    }
    const id =
        pendingDeleteDonationId;
    const tokenElement =
        document.querySelector(
            'input[name="__RequestVerificationToken"]'
        );
    if (!tokenElement) {
        alert(
            "Security token was not found. Please refresh the page."
        );
        return;
    }
    const token =
        tokenElement.value;
    const deleteButton =
        document.getElementById(
            "confirmDeleteButton"
        );
    const originalText =
        deleteButton
            ? deleteButton.innerHTML
            : "Delete Donation";
    try {
        if (deleteButton) {
            deleteButton.disabled = true;
            deleteButton.innerHTML =
                '<span class="verify-button-spinner"></span> Deleting...';
        }
        const formData =
            new FormData();
        formData.append(
            "__RequestVerificationToken",
            token
        );
        formData.append(
            "id",
            id
        );
        const response =
            await fetch(
                "/Admin/Donations/Delete",
                {
                    method: "POST",
                    headers: {
                        "X-Requested-With":
                            "XMLHttpRequest",
                        "Accept":
                            "application/json"
                    },
                    body: formData
                }
            );
        const responseText =
            await response.text();
        let result;
        try {
            result =
                JSON.parse(
                    responseText
                );
        }
        catch {
            throw new Error(
                "Server returned an unexpected response. HTTP Status: " +
                response.status
            );
        }
        if (!response.ok) {
            throw new Error(
                result.message ||
                "Unable to delete donation."
            );
        }
        if (!result.success) {
            throw new Error(
                result.message ||
                "Unable to delete donation."
            );
        }
        closeDeleteConfirmation();
        window.location.reload();
    }
    catch (error) {
        console.error(
            "Delete donation error:",
            error
        );
        if (deleteButton) {
            deleteButton.disabled = false;
            deleteButton.innerHTML =
                originalText;
        }
        alert(
            error.message ||
            "Unable to delete donation."
        );
    }
}
// =========================================================
// OFFLINE DONATION FORM
// =========================================================
(function () {
    const MAX_FILE_SIZE =
        500 * 1024;
    // =====================================================
    // HELPERS
    // =====================================================
    function getOfflineElement(id) {
        return document.getElementById(id);
    }
    function clearOfflineFileError() {
        const fileInput =
            getOfflineElement(
                "offlineVoucherInput"
            );
        const dropzone =
            getOfflineElement(
                "offlineVoucherDropzone"
            );
        const errorElement =
            document.querySelector(
                '[data-field-error="OfflineVoucher"]'
            );
        if (errorElement) {
            errorElement.textContent =
                "";
        }
        if (fileInput) {
            fileInput.classList.remove(
                "is-invalid"
            );
        }
    }
    function showOfflineFileError(message) {
        const fileInput =
            getOfflineElement(
                "offlineVoucherInput"
            );
        const errorElement =
            document.querySelector(
                '[data-field-error="OfflineVoucher"]'
            );
        if (errorElement) {
            errorElement.textContent =
                message;
        }
        if (fileInput) {
            fileInput.classList.add(
                "is-invalid"
            );
        }
    }
    function hideOfflineVoucherPreview() {
        const preview =
            getOfflineElement(
                "offlineVoucherPreview"
            );
        const fileName =
            getOfflineElement(
                "offlineVoucherFileName"
            );
        const fileSize =
            getOfflineElement(
                "offlineVoucherFileSize"
            );
        if (preview) {
            preview.style.display =
                "none";
        }
        if (fileName) {
            fileName.textContent =
                "";
        }
        if (fileSize) {
            fileSize.textContent =
                "";
        }
    }
    function formatOfflineFileSize(bytes) {
        if (bytes < 1024) {
            return bytes + " B";
        }
        return (
            bytes / 1024
        ).toFixed(1) + " KB";
    }
    // =====================================================
    // OFFLINE MODAL HEIGHT — CONTENT ONLY
    // =====================================================
    function syncOfflineDonationModalHeight() {
        const modal = document.getElementById("offlineDonationModal");
        const dialog = modal
            ? modal.querySelector(".offline-donation-modal-dialog")
            : null;
        const body = document.getElementById("offlineDonationModalBody");
        if (!dialog || !body) {
            return;
        }
        // Remove any old inline dimensions first.
        dialog.style.removeProperty("height");
        dialog.style.removeProperty("min-height");
        body.style.removeProperty("height");
        body.style.removeProperty("min-height");
        requestAnimationFrame(function () {
            const available = Math.max(180, window.innerHeight - 82);
            const contentHeight = body.scrollHeight;
            if (contentHeight > available) {
                body.style.height = available + "px";
            } else {
                body.style.height = contentHeight + "px";
            }
            dialog.style.height = "auto";
            dialog.style.minHeight = "0";
        });
    }
    // =====================================================
    // INITIALIZE OFFLINE DONATION FORM
    // =====================================================
    function initializeOfflineDonationForm() {
        const form =
            getOfflineElement(
                "offlineDonationForm"
            );
        if (!form) {
            return;
        }
        // Prevent duplicate event handlers.
        if (
            form.dataset.initialized ===
            "true"
        ) {
            return;
        }
        form.dataset.initialized =
            "true";
        const fileInput =
            getOfflineElement(
                "offlineVoucherInput"
            );
        const dropzone =
            getOfflineElement(
                "offlineVoucherDropzone"
            );
        const preview =
            getOfflineElement(
                "offlineVoucherPreview"
            );
        const fileName =
            getOfflineElement(
                "offlineVoucherFileName"
            );
        const fileSize =
            getOfflineElement(
                "offlineVoucherFileSize"
            );
        const removeButton =
            getOfflineElement(
                "offlineVoucherRemove"
            );
        const remarks =
            getOfflineElement(
                "offlineDonationRemarks"
            );
        const remarksCounter =
            getOfflineElement(
                "offlineRemarksCounter"
            );
        const submitButton =
            getOfflineElement(
                "submitOfflineDonationButton"
            );
        // =================================================
        // FILE PICKER BUTTON
        // =================================================
        if (dropzone && fileInput) {
            dropzone.addEventListener(
                "click",
                function () {
                    fileInput.click();
                }
            );
        }
        // =================================================
        // FILE UPLOAD
        // =================================================
        if (fileInput) {
            fileInput.addEventListener(
                "change",
                function () {
                    clearOfflineFileError();
                    hideOfflineVoucherPreview();
                    const file =
                        this.files &&
                            this.files.length
                            ? this.files[0]
                            : null;
                    if (!file) {
                        return;
                    }
                    // STRICTLY LESS THAN 500 KB
                    if (
                        file.size >=
                        MAX_FILE_SIZE
                    ) {
                        showOfflineFileError(
                            "File size must be less than 500 KB."
                        );
                        this.value = "";
                        syncOfflineDonationModalHeight();
                        return;
                    }
                    if (fileName) {
                        fileName.textContent =
                            file.name;
                    }
                    if (fileSize) {
                        fileSize.textContent =
                            formatOfflineFileSize(
                                file.size
                            );
                    }
                    if (preview) {
                        preview.style.display =
                            "flex";
                    }
                    syncOfflineDonationModalHeight();
                }
            );
        }
        // =================================================
        // REMOVE SELECTED FILE
        // =================================================
        if (removeButton) {
            removeButton.addEventListener(
                "click",
                function () {
                    if (fileInput) {
                        fileInput.value =
                            "";
                    }
                    clearOfflineFileError();
                    hideOfflineVoucherPreview();
                    syncOfflineDonationModalHeight();
                }
            );
        }
        // =================================================
        // REMARKS CHARACTER COUNTER
        // =================================================
        function updateRemarksCounter() {
            if (
                !remarks ||
                !remarksCounter
            ) {
                return;
            }
            remarksCounter.textContent =
                remarks.value.length;
        }
        if (remarks) {
            remarks.addEventListener(
                "input",
                updateRemarksCounter
            );
            updateRemarksCounter();
        }
        // =================================================
        // FORM SUBMIT
        // =================================================
        syncOfflineDonationModalHeight();
        form.addEventListener(
            "submit",
            async function (event) {
                event.preventDefault();
                // =================================================
                // CLEAR PREVIOUS VALIDATION
                // =================================================
                const validationSummary =
                    getOfflineElement(
                        "offlineDonationValidationSummary"
                    );
                if (validationSummary) {
                    validationSummary.innerHTML =
                        "";
                    validationSummary.style.display =
                        "none";
                }
                document
                    .querySelectorAll(
                        "#offlineDonationForm .offline-donation-error"
                    )
                    .forEach(
                        function (element) {
                            element.textContent =
                                "";
                        }
                    );
                document
                    .querySelectorAll(
                        "#offlineDonationForm .is-invalid"
                    )
                    .forEach(
                        function (element) {
                            element.classList.remove(
                                "is-invalid"
                            );
                        }
                    );
                // =================================================
                // FILE VALIDATION
                // =================================================
                if (fileInput) {
                    const file =
                        fileInput.files &&
                            fileInput.files.length
                            ? fileInput.files[0]
                            : null;
                    if (!file) {
                        showOfflineFileError(
                            "Please upload the offline donation voucher."
                        );
                        fileInput.focus();
                        return;
                    }
                    if (
                        file.size >=
                        MAX_FILE_SIZE
                    ) {
                        showOfflineFileError(
                            "File size must be less than 500 KB."
                        );
                        fileInput.focus();
                        return;
                    }
                }
                // =================================================
                // PREVENT DOUBLE SUBMISSION
                // =================================================
                if (submitButton) {
                    if (
                        submitButton.dataset.submitting ===
                        "true"
                    ) {
                        return;
                    }
                    submitButton.dataset.submitting =
                        "true";
                    submitButton.disabled =
                        true;
                    submitButton.innerHTML =
                        '<span class="offline-submit-icon">✓</span>' +
                        ' Collecting...';
                }
                try {
                    // =================================================
                    // FORM DATA
                    // =================================================
                    const formData =
                        new FormData(form);
                    // =================================================
                    // SUBMIT USING FETCH
                    // =================================================
                    const response =
                        await fetch(
                            form.action,
                            {
                                method: "POST",
                                headers: {
                                    "X-Requested-With":
                                        "XMLHttpRequest",
                                    "Accept":
                                        "application/json"
                                },
                                body: formData
                            }
                        );
                    // =================================================
                    // READ RESPONSE
                    // =================================================
                    const responseText =
                        await response.text();
                    let result;
                    try {
                        result =
                            JSON.parse(
                                responseText
                            );
                    }
                    catch {
                        throw new Error(
                            "Server returned an unexpected response. HTTP Status: " +
                            response.status
                        );
                    }
                    // =================================================
                    // SERVER ERROR
                    // =================================================
                    if (!response.ok) {
                        throw new Error(
                            result.message ||
                            "Unable to save offline donation."
                        );
                    }
                    // =================================================
                    // VALIDATION ERRORS
                    // =================================================
                    if (!result.success) {
                        // FIELD ERRORS
                        if (
                            Array.isArray(
                                result.errors
                            )
                        ) {
                            result.errors.forEach(
                                function (error) {
                                    if (
                                        !error ||
                                        !error.field
                                    ) {
                                        return;
                                    }
                                    const errorElement =
                                        document.querySelector(
                                            '#offlineDonationForm ' +
                                            '[data-field-error="' +
                                            error.field +
                                            '"]'
                                        );
                                    if (errorElement) {
                                        errorElement.textContent =
                                            error.message ||
                                            "Invalid value.";
                                    }
                                    const field =
                                        document.getElementById(
                                            error.field
                                        );
                                    if (field) {
                                        field.classList.add(
                                            "is-invalid"
                                        );
                                    }
                                }
                            );
                        }
                        // GENERAL ERROR
                        if (
                            result.message
                        ) {
                            if (validationSummary) {
                                validationSummary.textContent =
                                    result.message;
                                validationSummary.style.display =
                                    "block";
                            }
                        }
                        // RESET BUTTON
                        if (submitButton) {
                            submitButton.dataset.submitting =
                                "false";
                            submitButton.disabled =
                                false;
                            submitButton.innerHTML =
                                '<span class="offline-submit-icon">✓</span>' +
                                ' Collect Cash Donation';
                        }
                        return;
                    }
                    // =================================================
                    // SUCCESS
                    // =================================================
                    const donationNumber =
                        result.donationNumber ||
                        "";
                    const successMessage =
                        result.message ||
                        "Cash donation collected successfully.";
                    // =================================================
                    // CLOSE OFFLINE DONATION MODAL
                    // =================================================
                    closeOfflineDonationModal();
                    // =================================================
                    // SHOW SUCCESS MODAL
                    // =================================================
                    showDonationSuccessModal(
                        successMessage,
                        donationNumber
                    );
                }
                catch (error) {
                    console.error(
                        "Offline donation submission error:",
                        error
                    );
                    // RESET BUTTON
                    if (submitButton) {
                        submitButton.dataset.submitting =
                            "false";
                        submitButton.disabled =
                            false;
                        submitButton.innerHTML =
                            '<span class="offline-submit-icon">✓</span>' +
                            ' Collect Cash Donation';
                    }
                    // SHOW ERROR
                    if (validationSummary) {
                        validationSummary.textContent =
                            error.message ||
                            "Unable to save offline donation.";
                        validationSummary.style.display =
                            "block";
                    }
                    else {
                        alert(
                            error.message ||
                            "Unable to save offline donation."
                        );
                    }
                }
            }
        );
    }
    // =========================================================
    // PUBLIC INITIALIZER
    // =========================================================
    window.initializeOfflineDonationForm =
        initializeOfflineDonationForm;
    window.syncOfflineDonationModalHeight =
        syncOfflineDonationModalHeight;
    // =========================================================
    // DYNAMIC FORM EVENT
    // =========================================================
    document.addEventListener(
        "offlineDonationFormLoaded",
        function () {
            initializeOfflineDonationForm();
        }
    );
    // =========================================================
    // INITIAL PAGE LOAD
    // =========================================================
    if (
        document.readyState ===
        "loading"
    ) {
        document.addEventListener(
            "DOMContentLoaded",
            function () {
                initializeOfflineDonationForm();
            }
        );
    }
    else {
        initializeOfflineDonationForm();
    }
})();
// =========================================================
// DONATION SUCCESS MODAL
// =========================================================
function showDonationSuccessModal(
    message,
    donationNumber
) {
    const modal =
        document.getElementById(
            "donationSuccessModal"
        );
    const messageElement =
        document.getElementById(
            "donationSuccessMessage"
        );
    const numberElement =
        document.getElementById(
            "donationSuccessNumber"
        );
    const numberBox =
        document.getElementById(
            "donationSuccessNumberBox"
        );
    if (!modal) {
        alert(
            message ||
            "Donation collected successfully."
        );
        return;
    }
    if (messageElement) {
        messageElement.textContent =
            message ||
            "Cash donation collected successfully.";
    }
    if (numberElement) {
        numberElement.textContent =
            donationNumber ||
            "";
    }
    if (numberBox) {
        numberBox.style.display =
            donationNumber
                ? "block"
                : "none";
    }
    modal.classList.add("show");
    document.body.classList.add(
        "donation-modal-open"
    );
}
// =========================================================
// CLOSE DONATION SUCCESS MODAL
// =========================================================
function closeDonationSuccessModal() {
    const modal =
        document.getElementById(
            "donationSuccessModal"
        );
    if (modal) {
        modal.classList.remove(
            "show"
        );
    }
    document.body.classList.remove(
        "donation-modal-open"
    );
    // Refresh donation list after
    // closing the success modal.
    window.location.reload();
}
// =========================================================
// ESCAPE KEY - CLOSE MODALS
// =========================================================
document.addEventListener(
    "keydown",
    function (event) {
        if (event.key !== "Escape") {
            return;
        }
        const verifyModal =
            document.getElementById(
                "donationVerifyModal"
            );
        const rejectModal =
            document.getElementById(
                "donationRejectModal"
            );
        const detailsModal =
            document.getElementById(
                "donationDetailsModal"
            );
        const offlineModal =
            document.getElementById(
                "offlineDonationModal"
            );
        const deleteModal =
            document.getElementById(
                "donationDeleteModal"
            );
        const successModal =
            document.getElementById(
                "donationSuccessModal"
            );
        // -------------------------------------------------
        // SUCCESS
        // -------------------------------------------------
        if (
            successModal &&
            successModal.classList.contains("show")
        ) {
            closeDonationSuccessModal();
            return;
        }
        // -------------------------------------------------
        // VERIFY
        // -------------------------------------------------
        if (
            verifyModal &&
            verifyModal.classList.contains("show")
        ) {
            closeVerifyConfirmation();
            return;
        }
        // -------------------------------------------------
        // REJECT
        // -------------------------------------------------
        if (
            rejectModal &&
            rejectModal.classList.contains("show")
        ) {
            closeRejectConfirmation();
            return;
        }
        // -------------------------------------------------
        // DETAILS
        // -------------------------------------------------
        if (
            detailsModal &&
            detailsModal.classList.contains("show")
        ) {
            closeDonationDetailsModal();
            return;
        }
        // -------------------------------------------------
        // OFFLINE
        // -------------------------------------------------
        if (
            offlineModal &&
            offlineModal.classList.contains("show")
        ) {
            closeOfflineDonationModal();
            return;
        }
        // -------------------------------------------------
        // DELETE
        // -------------------------------------------------
        if (
            deleteModal &&
            deleteModal.classList.contains("show")
        ) {
            closeDeleteConfirmation();
        }
    }
);
// =========================================================
// REJECTION REMARKS CHARACTER COUNTER
// =========================================================
document.addEventListener(
    "input",
    function (event) {
        if (
            event.target &&
            event.target.id ===
            "rejectionRemarks"
        ) {
            updateRejectionRemarksCounter();
        }
    }
);