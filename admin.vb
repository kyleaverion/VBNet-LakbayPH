Imports System.Data
Imports System.Data.SqlClient
Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient
Imports MySqlConnector
Imports MySqlCommand = MySql.Data.MySqlClient.MySqlCommand
Imports MySqlConnection = MySql.Data.MySqlClient.MySqlConnection

Public Class AdminBookingInterface
    Inherits Form

    ' Database connection
    Dim connectionString As String = "Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;"
    Private connection As MySqlConnection

    ' UI Controls
    Private mainPanel As Panel
    Private headerPanel As Panel
    Private contentPanel As Panel
    Private sidePanel As Panel
    Private searchPanel As Panel
    Private gridPanel As Panel
    Private detailsPanel As Panel
    Private durationLabel As Label
    Private durationNumeric As NumericUpDown


    ' Header controls
    Private titleLabel As Label
    Private logoutButton As Button
    Private refreshButton As Button

    ' Side panel controls
    Private allBookingsButton As Button
    Private pendingBookingsButton As Button
    Private confirmedBookingsButton As Button
    Private completedBookingsButton As Button
    Private cancelledBookingsButton As Button
    Private financialManagementButton As Button

    ' Search controls
    Private WithEvents searchTextBox As TextBox
    Private searchButton As Button
    Private filterComboBox As ComboBox
    Private createNewPackageButton As Button
    Private newPackageDetailsGroupBox As GroupBox
    Private newPackageCategoryComboBox As ComboBox
    Private newPackageIdealForTextBox As TextBox
    Private newPackageInclusionsTextBox As TextBox
    Private newPackageHighlightsTextBox As TextBox
    Private newPackageFlagTextBox As TextBox
    Private WithEvents btnUploadImage As Button
    Private WithEvents PictureBoxImage As PictureBox
    Private txtImageName As TextBox
    Private currentImageFileName As String = ""
    Private selectedImagePath As String = ""
    Private OpenFileDialog1 As OpenFileDialog
    Private newPackageScrollPanel As Panel


    ' Data grid
    Private bookingsDataGrid As DataGridView
    Private packagesDataGrid As DataGridView

    ' Details panel controls
    Private detailsGroupBox As GroupBox
    Private bookingIdLabel As Label
    Private customerNameTextBox As TextBox
    Private customerEmailTextBox As TextBox
    Private customerPhoneTextBox As TextBox
    Private packageTitleLabel As Label
    Private travelDatePicker As DateTimePicker
    Private endDatePicker As DateTimePicker
    Private statusComboBox As ComboBox
    Private paymentStatusComboBox As ComboBox
    Private finalAmountTextBox As TextBox
    Private numberOfPeopleNumeric As NumericUpDown
    Private bookingReferenceLabel As Label
    Private paymentMethodLabel As Label
    Private packageLocationLabel As Label
    Private packageTypeLabel As Label
    Private updateButton As Button
    Private cancelButton As Button
    Private packageManagementButton As Button


    ' Package Management Controls
    Private packageDetailsGroupBox As GroupBox
    Private packageIdLabel As Label
    Private packageTitleTextBox As TextBox
    Private packageLocationTextBox As TextBox
    Private packageTypeComboBox As ComboBox
    Private packagePriceTextBox As TextBox
    Private packageSlotsNumeric As NumericUpDown
    Private packageStatusComboBox As ComboBox
    Private packageDescriptionTextBox As TextBox
    Private packageAddonsTextBox As TextBox
    Private packageStartDatePicker As DateTimePicker
    Private packageEndDatePicker As DateTimePicker
    Private WithEvents createPackageButton As Button
    Private WithEvents updatePackageButton As Button
    Private WithEvents deletePackageButton As Button
    Private WithEvents resetPackageButton As Button
    Private WithEvents currentPackageData As DataTable




    Private currentView As String = "bookings"
    Public Sub New()
        InitializeComponent()
        InitializeDatabase()
        LoadBookingsData()
    End Sub

    Private Sub InitializeComponent()
        ' Form properties
        Me.Text = "LakbayPH - Admin Booking Management"
        Me.Size = New Size(1400, 800)
        Me.MinimumSize = New Size(1200, 700)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(240, 244, 248)
        Me.Font = New Font("Segoe UI", 9, FontStyle.Regular)
        Me.DoubleBuffered = True


        CreateMainLayout()
        CreateHeader()
        CreateSidePanel()
        CreateSearchPanel()
        CreateDataGrid()
        CreateDetailsPanel()
        CreatePackageManagementControls()
    End Sub

    Private Sub CreateMainLayout()
        ' Main container panel
        mainPanel = New Panel()
        mainPanel.Dock = DockStyle.Fill
        mainPanel.BackColor = Color.FromArgb(240, 244, 248)
        Me.Controls.Add(mainPanel)

        ' Header panel
        headerPanel = New Panel()
        headerPanel.Height = 70
        headerPanel.Dock = DockStyle.Top
        headerPanel.BackColor = Color.FromArgb(37, 99, 235)
        mainPanel.Controls.Add(headerPanel)

        ' Side panel
        sidePanel = New Panel()
        sidePanel.Width = 250
        sidePanel.Dock = DockStyle.Left
        sidePanel.BackColor = Color.FromArgb(51, 65, 85)
        mainPanel.Controls.Add(sidePanel)

        ' Content panel 
        contentPanel = New Panel()
        contentPanel.Dock = DockStyle.Fill
        contentPanel.BackColor = Color.FromArgb(248, 250, 252)
        contentPanel.Padding = New Padding(0)
        mainPanel.Controls.Add(contentPanel)

        ' Search panel
        searchPanel = New Panel()
        searchPanel.Height = 60
        searchPanel.Dock = DockStyle.Top
        searchPanel.BackColor = Color.White
        searchPanel.Padding = New Padding(170, 15, 15, 15)
        contentPanel.Controls.Add(searchPanel)

        ' Details panel
        detailsPanel = New Panel()
        detailsPanel.Width = 400
        detailsPanel.Dock = DockStyle.Right
        detailsPanel.BackColor = Color.White
        detailsPanel.Padding = New Padding(15)
        contentPanel.Controls.Add(detailsPanel)

        ' Grid panel
        gridPanel = New Panel()
        gridPanel.Dock = DockStyle.Fill
        gridPanel.BackColor = Color.White
        gridPanel.Padding = New Padding(270, 95, 470, 60)
        contentPanel.Controls.Add(gridPanel)
    End Sub

    Private Sub CreateHeader()
        ' Title label
        titleLabel = New Label()
        titleLabel.Text = "LAKBAYPH ADMIN DASHBOARD"
        titleLabel.Font = New Font("Segoe UI", 16, FontStyle.Bold)
        titleLabel.ForeColor = Color.White
        titleLabel.Location = New Point(20, 20)
        titleLabel.AutoSize = True
        headerPanel.Controls.Add(titleLabel)

        ' Refresh button
        refreshButton = New Button()
        refreshButton.Text = "🔄 REFRESH"
        refreshButton.Size = New Size(120, 35)
        refreshButton.Location = New Point(headerPanel.Width - 280, 17)
        refreshButton.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        refreshButton.BackColor = Color.FromArgb(16, 185, 129)
        refreshButton.ForeColor = Color.White
        refreshButton.FlatStyle = FlatStyle.Flat
        refreshButton.FlatAppearance.BorderSize = 0
        refreshButton.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        refreshButton.Cursor = Cursors.Hand
        AddHandler refreshButton.Click, AddressOf RefreshButton_Click
        headerPanel.Controls.Add(refreshButton)

        ' Logout button
        logoutButton = New Button()
        logoutButton.Text = "🚪 LOGOUT"
        logoutButton.Size = New Size(120, 35)
        logoutButton.Location = New Point(headerPanel.Width - 150, 17)
        logoutButton.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        logoutButton.BackColor = Color.FromArgb(239, 68, 68)
        logoutButton.ForeColor = Color.White
        logoutButton.FlatStyle = FlatStyle.Flat
        logoutButton.FlatAppearance.BorderSize = 0
        logoutButton.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        logoutButton.Cursor = Cursors.Hand
        AddHandler logoutButton.Click, AddressOf LogoutButton_Click
        headerPanel.Controls.Add(logoutButton)
    End Sub

    Private Sub CreateSidePanel()
        Dim yPos As Integer = 30

        ' All Bookings Button
        allBookingsButton = CreateSideButton("📋 All Bookings", yPos)
        AddHandler allBookingsButton.Click, AddressOf AllBookingsButton_Click
        sidePanel.Controls.Add(allBookingsButton)
        yPos += 60

        ' Pending Bookings Button
        pendingBookingsButton = CreateSideButton("⏳ Pending", yPos)
        AddHandler pendingBookingsButton.Click, AddressOf PendingBookingsButton_Click
        sidePanel.Controls.Add(pendingBookingsButton)
        yPos += 60

        ' Confirmed Bookings Button
        confirmedBookingsButton = CreateSideButton("✅ Confirmed", yPos)
        AddHandler confirmedBookingsButton.Click, AddressOf ConfirmedBookingsButton_Click
        sidePanel.Controls.Add(confirmedBookingsButton)
        yPos += 60

        ' Completed Bookings Button
        completedBookingsButton = CreateSideButton("🎉 Completed", yPos)
        AddHandler completedBookingsButton.Click, AddressOf CompletedBookingsButton_Click
        sidePanel.Controls.Add(completedBookingsButton)
        yPos += 60

        ' Cancelled Bookings Button
        cancelledBookingsButton = CreateSideButton("❌ Cancelled", yPos)
        AddHandler cancelledBookingsButton.Click, AddressOf CancelledBookingsButton_Click
        sidePanel.Controls.Add(cancelledBookingsButton)
        yPos += 60

        ' Package Management Button
        packageManagementButton = CreateSideButton("📦 Package Manager", yPos)
        AddHandler packageManagementButton.Click, AddressOf PackageManagementButton_Click
        sidePanel.Controls.Add(packageManagementButton)
        yPos += 60

        ' Financial Management Button
        financialManagementButton = CreateSideButton("💰 Finance Manager", yPos)
        AddHandler financialManagementButton.Click, AddressOf FinancialManagementButton_Click
        sidePanel.Controls.Add(financialManagementButton)
        yPos += 60

        ' Create New Package Button
        createNewPackageButton = CreateSideButton("➕ Create Package", yPos)
        AddHandler createNewPackageButton.Click, AddressOf CreateNewPackageButton_Click
        sidePanel.Controls.Add(createNewPackageButton)
    End Sub

    Private Function CreateSideButton(text As String, yPos As Integer) As Button
        Dim btn As New Button()
        btn.Text = text
        btn.Size = New Size(230, 45)
        btn.Location = New Point(10, yPos)
        btn.BackColor = Color.FromArgb(71, 85, 105)
        btn.ForeColor = Color.White
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        btn.TextAlign = ContentAlignment.MiddleLeft
        btn.Padding = New Padding(15, 0, 0, 0)
        btn.Cursor = Cursors.Hand
        Return btn
    End Function

    Private Sub CreateSearchPanel()
        ' Search textbox
        searchTextBox = New TextBox()
        searchTextBox.Size = New Size(250, 30)
        searchTextBox.Location = New Point(15, 15)
        searchTextBox.Font = New Font("Segoe UI", 10)
        searchPanel.Controls.Add(searchTextBox)

        ' Filter combo box
        filterComboBox = New ComboBox()
        filterComboBox.Size = New Size(150, 30)
        filterComboBox.Location = New Point(280, 15)
        filterComboBox.Font = New Font("Segoe UI", 10)
        filterComboBox.DropDownStyle = ComboBoxStyle.DropDownList
        filterComboBox.Items.AddRange({"All Packages", "Freediving", "Domestic", "International"})
        filterComboBox.SelectedIndex = 0
        searchPanel.Controls.Add(filterComboBox)

        ' Search button
        searchButton = New Button()
        searchButton.Text = "🔍 SEARCH"
        searchButton.Size = New Size(100, 30)
        searchButton.Location = New Point(450, 15)
        searchButton.BackColor = Color.FromArgb(37, 99, 235)
        searchButton.ForeColor = Color.White
        searchButton.FlatStyle = FlatStyle.Flat
        searchButton.FlatAppearance.BorderSize = 0
        searchButton.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        searchButton.Cursor = Cursors.Hand
        AddHandler searchButton.Click, AddressOf SearchButton_Click
        searchPanel.Controls.Add(searchButton)
    End Sub

    Private Sub CreateDataGrid()
        bookingsDataGrid = New DataGridView()
        bookingsDataGrid.Dock = DockStyle.Fill
        bookingsDataGrid.BackgroundColor = Color.White
        bookingsDataGrid.BorderStyle = BorderStyle.None
        bookingsDataGrid.Margin = New Padding(1, 0, 0, 0)
        bookingsDataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        bookingsDataGrid.MultiSelect = False
        bookingsDataGrid.AllowUserToAddRows = False
        bookingsDataGrid.AllowUserToDeleteRows = False
        bookingsDataGrid.ReadOnly = True
        bookingsDataGrid.RowHeadersVisible = False
        bookingsDataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        bookingsDataGrid.Font = New Font("Segoe UI", 9)
        bookingsDataGrid.ColumnHeadersHeight = 50
        bookingsDataGrid.RowTemplate.Height = 35

        ' DataGridView unresizable and unmovable
        bookingsDataGrid.AllowUserToResizeColumns = False
        bookingsDataGrid.AllowUserToResizeRows = False
        bookingsDataGrid.AllowUserToOrderColumns = False
        bookingsDataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing

        ' Style grid
        bookingsDataGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(51, 65, 85)
        bookingsDataGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        bookingsDataGrid.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        bookingsDataGrid.ColumnHeadersDefaultCellStyle.Padding = New Padding(5)
        bookingsDataGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252)
        bookingsDataGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235)

        AddHandler bookingsDataGrid.SelectionChanged, AddressOf BookingsDataGrid_SelectionChanged
        gridPanel.Controls.Add(bookingsDataGrid)

        packagesDataGrid = New DataGridView()
        packagesDataGrid.Dock = DockStyle.Fill
        packagesDataGrid.BackgroundColor = Color.White
        packagesDataGrid.BorderStyle = BorderStyle.None
        packagesDataGrid.Margin = New Padding(1, 0, 0, 0)
        packagesDataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        packagesDataGrid.MultiSelect = False
        packagesDataGrid.AllowUserToAddRows = False
        packagesDataGrid.AllowUserToDeleteRows = False
        packagesDataGrid.ReadOnly = True
        packagesDataGrid.RowHeadersVisible = False
        packagesDataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        packagesDataGrid.Font = New Font("Segoe UI", 9)
        packagesDataGrid.ColumnHeadersHeight = 50
        packagesDataGrid.RowTemplate.Height = 35
        packagesDataGrid.AllowUserToResizeColumns = False
        packagesDataGrid.AllowUserToResizeRows = False
        packagesDataGrid.AllowUserToOrderColumns = False
        packagesDataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        packagesDataGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(51, 65, 85)
        packagesDataGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        packagesDataGrid.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        packagesDataGrid.ColumnHeadersDefaultCellStyle.Padding = New Padding(5)
        packagesDataGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252)
        packagesDataGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235)
        packagesDataGrid.Visible = False
        AddHandler packagesDataGrid.SelectionChanged, AddressOf PackagesDataGrid_SelectionChanged
        gridPanel.Controls.Add(packagesDataGrid)
    End Sub

    ' Booking details panel
    Private Sub CreateDetailsPanel()
        ' Details group box
        detailsGroupBox = New GroupBox()
        detailsGroupBox.Location = New Point(15, 50)
        detailsGroupBox.Text = "Booking Details"
        detailsGroupBox.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        detailsGroupBox.ForeColor = Color.FromArgb(51, 65, 85)
        detailsGroupBox.Dock = DockStyle.Fill
        detailsPanel.Controls.Add(detailsGroupBox)

        Dim yPos As Integer = 100
        Dim spacing As Integer = 45

        ' Booking ID (read-only)
        bookingIdLabel = New Label()
        bookingIdLabel.Text = "Booking ID: --"
        bookingIdLabel.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        bookingIdLabel.Location = New Point(15, yPos)
        bookingIdLabel.Size = New Size(350, 25)
        detailsGroupBox.Controls.Add(bookingIdLabel)
        yPos += spacing

        ' Booking Reference (read-only)
        bookingReferenceLabel = New Label()
        bookingReferenceLabel.Text = "Reference: --"
        bookingReferenceLabel.Font = New Font("Segoe UI", 9)
        bookingReferenceLabel.Location = New Point(15, yPos)
        bookingReferenceLabel.Size = New Size(350, 25)
        bookingReferenceLabel.ForeColor = Color.FromArgb(107, 114, 128)
        detailsGroupBox.Controls.Add(bookingReferenceLabel)
        yPos += 30

        ' Customer Name
        CreateDetailLabel("Customer Name:", yPos - 20)
        customerNameTextBox = CreateDetailTextBox(yPos)
        detailsGroupBox.Controls.Add(customerNameTextBox)
        yPos += spacing

        ' Customer Email
        CreateDetailLabel("Email:", yPos - 20)
        customerEmailTextBox = CreateDetailTextBox(yPos)
        detailsGroupBox.Controls.Add(customerEmailTextBox)
        yPos += spacing

        ' Customer Phone
        CreateDetailLabel("Phone:", yPos - 20)
        customerPhoneTextBox = CreateDetailTextBox(yPos)
        detailsGroupBox.Controls.Add(customerPhoneTextBox)
        yPos += spacing

        ' Package Title (read-only)
        packageTitleLabel = New Label()
        packageTitleLabel.Text = "Package: --"
        packageTitleLabel.Font = New Font("Segoe UI", 9)
        packageTitleLabel.Location = New Point(15, yPos)
        packageTitleLabel.Size = New Size(350, 20)
        packageTitleLabel.ForeColor = Color.FromArgb(107, 114, 128)
        detailsGroupBox.Controls.Add(packageTitleLabel)
        yPos += 25

        ' Package Location (read-only)
        packageLocationLabel = New Label()
        packageLocationLabel.Text = "Location: --"
        packageLocationLabel.Font = New Font("Segoe UI", 9)
        packageLocationLabel.Location = New Point(15, yPos)
        packageLocationLabel.Size = New Size(350, 20)
        packageLocationLabel.ForeColor = Color.FromArgb(107, 114, 128)
        detailsGroupBox.Controls.Add(packageLocationLabel)
        yPos += 25

        ' Package Type (read-only)
        packageTypeLabel = New Label()
        packageTypeLabel.Text = "Type: --"
        packageTypeLabel.Font = New Font("Segoe UI", 9)
        packageTypeLabel.Location = New Point(15, yPos)
        packageTypeLabel.Size = New Size(350, 20)
        packageTypeLabel.ForeColor = Color.FromArgb(107, 114, 128)
        detailsGroupBox.Controls.Add(packageTypeLabel)
        yPos += 65

        ' Travel Date
        CreateDetailLabel("Travel Date:", yPos - 20)
        travelDatePicker = New DateTimePicker()
        travelDatePicker.Location = New Point(15, yPos)
        travelDatePicker.Size = New Size(145, 25)
        travelDatePicker.Font = New Font("Segoe UI", 9)
        detailsGroupBox.Controls.Add(travelDatePicker)

        ' End Date
        endDatePicker = New DateTimePicker()
        endDatePicker.Location = New Point(190, yPos)
        endDatePicker.Size = New Size(145, 25)
        endDatePicker.Font = New Font("Segoe UI", 9)
        detailsGroupBox.Controls.Add(endDatePicker)
        yPos += spacing

        ' Number of People
        CreateDetailLabel("Number of People:", yPos - 20)
        numberOfPeopleNumeric = New NumericUpDown()
        numberOfPeopleNumeric.Location = New Point(15, yPos)
        numberOfPeopleNumeric.Size = New Size(100, 25)
        numberOfPeopleNumeric.Font = New Font("Segoe UI", 9)
        numberOfPeopleNumeric.Minimum = 1
        numberOfPeopleNumeric.Maximum = 50
        detailsGroupBox.Controls.Add(numberOfPeopleNumeric)
        yPos += spacing

        ' Final Amount
        CreateDetailLabel("Final Amount (PHP):", yPos - 20)
        finalAmountTextBox = CreateDetailTextBox(yPos)
        detailsGroupBox.Controls.Add(finalAmountTextBox)
        yPos += spacing

        ' Status
        CreateDetailLabel("Booking Status:", yPos - 20)
        statusComboBox = New ComboBox()
        statusComboBox.Location = New Point(15, yPos)
        statusComboBox.Size = New Size(145, 25)
        statusComboBox.Font = New Font("Segoe UI", 9)
        statusComboBox.DropDownStyle = ComboBoxStyle.DropDownList
        statusComboBox.Items.AddRange({"Pending", "Confirmed", "Cancelled", "Completed"})
        detailsGroupBox.Controls.Add(statusComboBox)

        ' Payment Status
        paymentStatusComboBox = New ComboBox()
        paymentStatusComboBox.Location = New Point(190, yPos)
        paymentStatusComboBox.Size = New Size(145, 25)
        paymentStatusComboBox.Font = New Font("Segoe UI", 9)
        paymentStatusComboBox.DropDownStyle = ComboBoxStyle.DropDownList
        paymentStatusComboBox.Items.AddRange({"Pending", "Paid", "Cancelled"})
        detailsGroupBox.Controls.Add(paymentStatusComboBox)
        yPos += spacing

        ' Payment Method (read-only)
        paymentMethodLabel = New Label()
        paymentMethodLabel.Text = "Payment Method: --"
        paymentMethodLabel.Font = New Font("Segoe UI", 9)
        paymentMethodLabel.Location = New Point(15, yPos)
        paymentMethodLabel.Size = New Size(350, 20)
        paymentMethodLabel.ForeColor = Color.FromArgb(107, 114, 128)
        detailsGroupBox.Controls.Add(paymentMethodLabel)
        yPos += 30

        ' Update Button
        updateButton = New Button()
        updateButton.Text = "💾 UPDATE BOOKING"
        updateButton.Size = New Size(160, 40)
        updateButton.Location = New Point(15, yPos)
        updateButton.BackColor = Color.FromArgb(16, 185, 129)
        updateButton.ForeColor = Color.White
        updateButton.FlatStyle = FlatStyle.Flat
        updateButton.FlatAppearance.BorderSize = 0
        updateButton.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        updateButton.Cursor = Cursors.Hand
        AddHandler updateButton.Click, AddressOf UpdateButton_Click
        detailsGroupBox.Controls.Add(updateButton)

        ' Cancel Button
        cancelButton = New Button()
        cancelButton.Text = "↶ RESET"
        cancelButton.Size = New Size(160, 40)
        cancelButton.Location = New Point(185, yPos)
        cancelButton.BackColor = Color.FromArgb(107, 114, 128)
        cancelButton.ForeColor = Color.White
        cancelButton.FlatStyle = FlatStyle.Flat
        cancelButton.FlatAppearance.BorderSize = 0
        cancelButton.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        cancelButton.Cursor = Cursors.Hand
        AddHandler cancelButton.Click, AddressOf CancelButton_Click
        detailsGroupBox.Controls.Add(cancelButton)
    End Sub

    ' Package Management Controls
    Private Sub CreatePackageManagementControls()
        ' Package details group box
        packageDetailsGroupBox = New GroupBox()
        packageDetailsGroupBox.Location = New Point(15, 50)
        packageDetailsGroupBox.Text = "Package Details"
        packageDetailsGroupBox.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        packageDetailsGroupBox.ForeColor = Color.FromArgb(51, 65, 85)
        packageDetailsGroupBox.Dock = DockStyle.Fill
        packageDetailsGroupBox.Visible = False
        detailsPanel.Controls.Add(packageDetailsGroupBox)

        Dim yPos As Integer = 100
        Dim spacing As Integer = 45

        ' Package ID (read-only)
        packageIdLabel = New Label()
        packageIdLabel.Text = "Package ID: --"
        packageIdLabel.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        packageIdLabel.Location = New Point(15, yPos)
        packageIdLabel.Size = New Size(350, 25)
        packageDetailsGroupBox.Controls.Add(packageIdLabel)
        yPos += spacing

        ' Package Title
        CreatePackageDetailLabel("Package Title:", yPos)
        packageTitleTextBox = CreatePackageDetailTextBox(yPos)
        packageDetailsGroupBox.Controls.Add(packageTitleTextBox)
        yPos += spacing

        ' Package Location
        CreatePackageDetailLabel("Location:", yPos - 20)
        packageLocationTextBox = CreatePackageDetailTextBox(yPos)
        packageDetailsGroupBox.Controls.Add(packageLocationTextBox)
        yPos += spacing

        ' Package Type
        CreatePackageDetailLabel("Package Type:", yPos - 20)
        packageTypeComboBox = New ComboBox()
        packageTypeComboBox.Location = New Point(15, yPos)
        packageTypeComboBox.Size = New Size(330, 25)
        packageTypeComboBox.Font = New Font("Segoe UI", 9)
        packageTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList
        packageTypeComboBox.Items.AddRange({"Freediving", "Domestic", "International"})
        packageDetailsGroupBox.Controls.Add(packageTypeComboBox)
        yPos += spacing

        ' Package Price
        CreatePackageDetailLabel("Price (PHP):", yPos - 20)
        packagePriceTextBox = CreatePackageDetailTextBox(yPos)
        packageDetailsGroupBox.Controls.Add(packagePriceTextBox)
        yPos += spacing

        ' Available Slots
        CreatePackageDetailLabel("Available Slots:", yPos - 20)
        packageSlotsNumeric = New NumericUpDown()
        packageSlotsNumeric.Location = New Point(15, yPos)
        packageSlotsNumeric.Size = New Size(100, 25)
        packageSlotsNumeric.Font = New Font("Segoe UI", 9)
        packageSlotsNumeric.Minimum = 1
        packageSlotsNumeric.Maximum = 100
        packageDetailsGroupBox.Controls.Add(packageSlotsNumeric)
        yPos += spacing

        ' Package Status
        CreatePackageDetailLabel("Status:", yPos - 20)
        packageStatusComboBox = New ComboBox()
        packageStatusComboBox.Location = New Point(15, yPos)
        packageStatusComboBox.Size = New Size(145, 25)
        packageStatusComboBox.Font = New Font("Segoe UI", 9)
        packageStatusComboBox.DropDownStyle = ComboBoxStyle.DropDownList
        packageStatusComboBox.Items.AddRange({"Active", "Inactive"})
        packageDetailsGroupBox.Controls.Add(packageStatusComboBox)
        yPos += spacing

        ' Available Dates
        CreatePackageDetailLabel("Available From:", yPos - 20)
        packageStartDatePicker = New DateTimePicker()
        packageStartDatePicker.Location = New Point(15, yPos)
        packageStartDatePicker.Size = New Size(145, 25)
        packageStartDatePicker.Font = New Font("Segoe UI", 9)
        AddHandler packageStartDatePicker.ValueChanged, AddressOf CalculateDuration
        packageDetailsGroupBox.Controls.Add(packageStartDatePicker)

        packageEndDatePicker = New DateTimePicker()
        packageEndDatePicker.Location = New Point(190, yPos)
        packageEndDatePicker.Size = New Size(145, 25)
        packageEndDatePicker.Font = New Font("Segoe UI", 9)
        AddHandler packageEndDatePicker.ValueChanged, AddressOf CalculateDuration
        packageDetailsGroupBox.Controls.Add(packageEndDatePicker)
        yPos += spacing

        ' Duration (only enabled during creation)
        CreatePackageDetailLabel("Duration (days):", yPos - 20)
        durationNumeric = New NumericUpDown()
        durationNumeric.Location = New Point(15, yPos)
        durationNumeric.Size = New Size(100, 25)
        durationNumeric.Font = New Font("Segoe UI", 9)
        durationNumeric.Minimum = 1
        durationNumeric.Maximum = 365
        durationNumeric.Enabled = False ' Disabled by default
        packageDetailsGroupBox.Controls.Add(durationNumeric)

        durationLabel = New Label()
        durationLabel.Text = "0 days"
        durationLabel.Location = New Point(130, yPos)
        durationLabel.Size = New Size(200, 25)
        durationLabel.Font = New Font("Segoe UI", 9)
        packageDetailsGroupBox.Controls.Add(durationLabel)
        yPos += spacing

        ' Description
        CreatePackageDetailLabel("Description:", yPos - 20)
        packageDescriptionTextBox = New TextBox()
        packageDescriptionTextBox.Location = New Point(15, yPos)
        packageDescriptionTextBox.Size = New Size(330, 60)
        packageDescriptionTextBox.Font = New Font("Segoe UI", 9)
        packageDescriptionTextBox.Multiline = True
        packageDescriptionTextBox.ScrollBars = ScrollBars.Vertical
        packageDetailsGroupBox.Controls.Add(packageDescriptionTextBox)
        yPos += 90

        ' Available Addons
        CreatePackageDetailLabel("Available Addons (comma-separated):", yPos - 20)
        packageAddonsTextBox = New TextBox()
        packageAddonsTextBox.Location = New Point(15, yPos)
        packageAddonsTextBox.Size = New Size(330, 40)
        packageAddonsTextBox.Font = New Font("Segoe UI", 9)
        packageAddonsTextBox.Multiline = True
        packageDetailsGroupBox.Controls.Add(packageAddonsTextBox)
        yPos += 55

        updatePackageButton = New Button()
        updatePackageButton.Text = "💾 UPDATE"
        updatePackageButton.Size = New Size(80, 35)
        updatePackageButton.Location = New Point(105, yPos)
        updatePackageButton.BackColor = Color.FromArgb(37, 99, 235)
        updatePackageButton.ForeColor = Color.White
        updatePackageButton.FlatStyle = FlatStyle.Flat
        updatePackageButton.FlatAppearance.BorderSize = 0
        updatePackageButton.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        updatePackageButton.Cursor = Cursors.Hand
        AddHandler updatePackageButton.Click, AddressOf UpdatePackageButton_Click
        packageDetailsGroupBox.Controls.Add(updatePackageButton)

        deletePackageButton = New Button()
        deletePackageButton.Text = "🗑️ DELETE"
        deletePackageButton.Size = New Size(80, 35)
        deletePackageButton.Location = New Point(195, yPos)
        deletePackageButton.BackColor = Color.FromArgb(239, 68, 68)
        deletePackageButton.ForeColor = Color.White
        deletePackageButton.FlatStyle = FlatStyle.Flat
        deletePackageButton.FlatAppearance.BorderSize = 0
        deletePackageButton.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        deletePackageButton.Cursor = Cursors.Hand
        AddHandler deletePackageButton.Click, AddressOf DeletePackageButton_Click
        packageDetailsGroupBox.Controls.Add(deletePackageButton)

        resetPackageButton = New Button()
        resetPackageButton.Text = "↶ RESET"
        resetPackageButton.Size = New Size(80, 35)
        resetPackageButton.Location = New Point(285, yPos)
        resetPackageButton.BackColor = Color.FromArgb(107, 114, 128)
        resetPackageButton.ForeColor = Color.White
        resetPackageButton.FlatStyle = FlatStyle.Flat
        resetPackageButton.FlatAppearance.BorderSize = 0
        resetPackageButton.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        resetPackageButton.Cursor = Cursors.Hand
        AddHandler resetPackageButton.Click, AddressOf ResetPackageButton_Click
        packageDetailsGroupBox.Controls.Add(resetPackageButton)

        ' New Package Details Group Box
        newPackageDetailsGroupBox = New GroupBox()
        newPackageDetailsGroupBox.Text = "Create New Package"
        newPackageDetailsGroupBox.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        newPackageDetailsGroupBox.ForeColor = Color.FromArgb(51, 65, 85)
        newPackageDetailsGroupBox.Dock = DockStyle.Fill
        newPackageDetailsGroupBox.Visible = False
        detailsPanel.Controls.Add(newPackageDetailsGroupBox)

        ' Create scrollable panel inside the group box
        newPackageScrollPanel = New Panel()
        newPackageScrollPanel.Location = New Point(15, 50)
        newPackageScrollPanel.Size = New Size(newPackageDetailsGroupBox.Width - 20, newPackageDetailsGroupBox.Height - 20)
        newPackageScrollPanel.AutoScroll = True
        newPackageScrollPanel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom
        newPackageDetailsGroupBox.Controls.Add(newPackageScrollPanel)

        Dim newYPos As Integer = 30
        Dim newSpacing As Integer = 45

        ' Category Selection
        CreateNewPackageDetailLabel("Category:", newYPos - 20)
        newPackageCategoryComboBox = New ComboBox()
        newPackageCategoryComboBox.Location = New Point(15, newYPos)
        newPackageCategoryComboBox.Size = New Size(300, 25)
        newPackageCategoryComboBox.Font = New Font("Segoe UI", 9)
        newPackageCategoryComboBox.DropDownStyle = ComboBoxStyle.DropDownList
        LoadCategories()
        newPackageScrollPanel.Controls.Add(newPackageCategoryComboBox)
        newYPos += newSpacing

        ' Package Title
        CreateNewPackageDetailLabel("Package Title:", newYPos - 20)
        packageTitleTextBox = CreateNewPackageDetailTextBox(newYPos)
        newPackageScrollPanel.Controls.Add(packageTitleTextBox)
        newYPos += newSpacing

        ' Location
        CreateNewPackageDetailLabel("Location:", newYPos - 20)
        packageLocationTextBox = CreateNewPackageDetailTextBox(newYPos)
        newPackageScrollPanel.Controls.Add(packageLocationTextBox)
        newYPos += newSpacing

        ' Package Type (auto-filled based on category)
        CreateNewPackageDetailLabel("Package Type:", newYPos - 20)
        packageTypeComboBox = New ComboBox()
        packageTypeComboBox.Location = New Point(15, newYPos)
        packageTypeComboBox.Size = New Size(300, 25)
        packageTypeComboBox.Font = New Font("Segoe UI", 9)
        packageTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList
        packageTypeComboBox.Items.AddRange({"Freediving", "Domestic", "International"})
        packageTypeComboBox.Enabled = False ' Auto-filled
        newPackageScrollPanel.Controls.Add(packageTypeComboBox)
        newYPos += newSpacing

        ' Price
        CreateNewPackageDetailLabel("Price (PHP):", newYPos - 20)
        packagePriceTextBox = CreateNewPackageDetailTextBox(newYPos)
        newPackageScrollPanel.Controls.Add(packagePriceTextBox)
        newYPos += newSpacing

        ' Duration
        Dim duraLabel As New Label()
        duraLabel.Text = "Duration (days):"
        duraLabel.Location = New Point(15, newYPos - 20)
        duraLabel.AutoSize = True
        duraLabel.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        newPackageScrollPanel.Controls.Add(duraLabel)

        durationNumeric = New NumericUpDown()
        durationNumeric.Location = New Point(15, newYPos)
        durationNumeric.Size = New Size(80, 25)
        durationNumeric.Font = New Font("Segoe UI", 9)
        durationNumeric.Minimum = 1
        durationNumeric.Maximum = 365
        newPackageScrollPanel.Controls.Add(durationNumeric)

        ' Available Slots
        Dim slotsLabel As New Label()
        slotsLabel.Text = "Available Slots:"
        slotsLabel.Location = New Point(120, newYPos - 20)
        slotsLabel.AutoSize = True
        slotsLabel.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        newPackageScrollPanel.Controls.Add(slotsLabel)

        packageSlotsNumeric = New NumericUpDown()
        packageSlotsNumeric.Location = New Point(120, newYPos)
        packageSlotsNumeric.Size = New Size(80, 25)
        packageSlotsNumeric.Font = New Font("Segoe UI", 9)
        packageSlotsNumeric.Minimum = 1
        packageSlotsNumeric.Maximum = 120
        newPackageScrollPanel.Controls.Add(packageSlotsNumeric)

        ' Flag Symbol
        Dim flgLabel As New Label()
        flgLabel.Text = "Flag Symbol:"
        flgLabel.Location = New Point(230, newYPos - 20)
        flgLabel.AutoSize = True
        flgLabel.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        newPackageScrollPanel.Controls.Add(flgLabel)

        newPackageFlagTextBox = New TextBox()
        newPackageFlagTextBox.Location = New Point(230, newYPos)
        newPackageFlagTextBox.Size = New Size(80, 25)
        newPackageFlagTextBox.Font = New Font("Segoe UI", 9)
        newPackageScrollPanel.Controls.Add(newPackageFlagTextBox)
        newYPos += newSpacing

        ' Ideal For
        CreateNewPackageDetailLabel("Ideal For:", newYPos - 20)
        newPackageIdealForTextBox = CreateNewPackageDetailTextBox(newYPos)
        newPackageScrollPanel.Controls.Add(newPackageIdealForTextBox)
        newYPos += newSpacing

        ' Description
        CreateNewPackageDetailLabel("Description:", newYPos - 20)
        packageDescriptionTextBox = New TextBox()
        packageDescriptionTextBox.Location = New Point(15, newYPos)
        packageDescriptionTextBox.Size = New Size(300, 50)
        packageDescriptionTextBox.Font = New Font("Segoe UI", 9)
        packageDescriptionTextBox.Multiline = True
        packageDescriptionTextBox.ScrollBars = ScrollBars.Vertical
        newPackageScrollPanel.Controls.Add(packageDescriptionTextBox)
        newYPos += 70

        ' Inclusions
        CreateNewPackageDetailLabel("Inclusions (one per line):", newYPos - 20)
        newPackageInclusionsTextBox = New TextBox()
        newPackageInclusionsTextBox.Location = New Point(15, newYPos)
        newPackageInclusionsTextBox.Size = New Size(300, 50)
        newPackageInclusionsTextBox.Font = New Font("Segoe UI", 9)
        newPackageInclusionsTextBox.Multiline = True
        newPackageInclusionsTextBox.ScrollBars = ScrollBars.Vertical
        newPackageScrollPanel.Controls.Add(newPackageInclusionsTextBox)
        newYPos += 70

        ' Highlights
        CreateNewPackageDetailLabel("Highlights (one per line):", newYPos - 20)
        newPackageHighlightsTextBox = New TextBox()
        newPackageHighlightsTextBox.Location = New Point(15, newYPos)
        newPackageHighlightsTextBox.Size = New Size(300, 50)
        newPackageHighlightsTextBox.Font = New Font("Segoe UI", 9)
        newPackageHighlightsTextBox.Multiline = True
        newPackageHighlightsTextBox.ScrollBars = ScrollBars.Vertical
        newPackageScrollPanel.Controls.Add(newPackageHighlightsTextBox)
        newYPos += 70

        ' Image Upload Button
        btnUploadImage = New Button()
        btnUploadImage.Text = "📷 Upload Image"
        btnUploadImage.Size = New Size(150, 30)
        btnUploadImage.Location = New Point(15, newYPos)
        btnUploadImage.BackColor = Color.FromArgb(59, 130, 246)
        btnUploadImage.ForeColor = Color.White
        btnUploadImage.FlatStyle = FlatStyle.Flat
        btnUploadImage.FlatAppearance.BorderSize = 0
        btnUploadImage.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        btnUploadImage.Cursor = Cursors.Hand
        AddHandler btnUploadImage.Click, AddressOf btnUploadImage_Click
        newPackageScrollPanel.Controls.Add(btnUploadImage)

        ' Image Name TextBox
        txtImageName = New PlaceholderTextBox() With {
    .Size = New Size(150, 30),
    .Location = New Point(175, newYPos),
    .Font = New Font("Segoe UI", 9),
    .ReadOnly = True,
    .PlaceholderText = "Image filename"
}
        newPackageScrollPanel.Controls.Add(txtImageName)

        ' PictureBox for image preview
        PictureBoxImage = New PictureBox()
        PictureBoxImage.Location = New Point(15, newYPos + 40)
        PictureBoxImage.Size = New Size(300, 150)
        PictureBoxImage.SizeMode = PictureBoxSizeMode.Zoom
        PictureBoxImage.BorderStyle = BorderStyle.FixedSingle
        PictureBoxImage.BackColor = Color.FromArgb(240, 244, 248)
        newPackageScrollPanel.Controls.Add(PictureBoxImage)

        ' Initialize OpenFileDialog
        OpenFileDialog1 = New OpenFileDialog()
        OpenFileDialog1.Title = "Select Package Image"
        OpenFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"

        newYPos += 200

        ' Buttons
        createPackageButton = New Button()
        createPackageButton.Text = "➕ CREATE PACKAGE"
        createPackageButton.Size = New Size(120, 40)
        createPackageButton.Location = New Point(15, newYPos)
        createPackageButton.BackColor = Color.FromArgb(16, 185, 129)
        createPackageButton.ForeColor = Color.White
        createPackageButton.FlatStyle = FlatStyle.Flat
        createPackageButton.FlatAppearance.BorderSize = 0
        createPackageButton.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        createPackageButton.Cursor = Cursors.Hand
        AddHandler createPackageButton.Click, AddressOf CreateCompletePackageButton_Click
        newPackageScrollPanel.Controls.Add(createPackageButton)

        resetPackageButton = New Button()
        resetPackageButton.Text = "↶ RESET"
        resetPackageButton.Size = New Size(120, 40)
        resetPackageButton.Location = New Point(185, newYPos)
        resetPackageButton.BackColor = Color.FromArgb(107, 114, 128)
        resetPackageButton.ForeColor = Color.White
        resetPackageButton.FlatStyle = FlatStyle.Flat
        resetPackageButton.FlatAppearance.BorderSize = 0
        resetPackageButton.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        resetPackageButton.Cursor = Cursors.Hand
        AddHandler resetPackageButton.Click, AddressOf ResetNewPackageButton_Click
        newPackageScrollPanel.Controls.Add(resetPackageButton)

        newPackageScrollPanel.AutoScrollMinSize = New Size(0, newYPos + 100)
    End Sub

    ' Add this class to your project (put it before your main form class)
    Public Class PlaceholderTextBox
        Inherits TextBox

        Private _placeholderText As String
        Private _placeholderColor As Color = Color.Gray
        Private _isPlaceholderActive As Boolean = False

        Public Property PlaceholderText As String
            Get
                Return _placeholderText
            End Get
            Set(value As String)
                _placeholderText = value
                If String.IsNullOrEmpty(Me.Text) Then
                    ShowPlaceholder()
                End If
            End Set
        End Property

        Public Property PlaceholderColor As Color
            Get
                Return _placeholderColor
            End Get
            Set(value As Color)
                _placeholderColor = value
            End Set
        End Property

        Private Sub ShowPlaceholder()
            If Not String.IsNullOrEmpty(_placeholderText) AndAlso Not Me.Focused Then
                Me.Text = _placeholderText
                Me.ForeColor = _placeholderColor
                _isPlaceholderActive = True
            End If
        End Sub

        Private Sub HidePlaceholder()
            If _isPlaceholderActive Then
                Me.Text = String.Empty
                Me.ForeColor = SystemColors.WindowText
                _isPlaceholderActive = False
            End If
        End Sub

        Protected Overrides Sub OnGotFocus(e As EventArgs)
            HidePlaceholder()
            MyBase.OnGotFocus(e)
        End Sub

        Protected Overrides Sub OnLostFocus(e As EventArgs)
            If String.IsNullOrEmpty(Me.Text) Then
                ShowPlaceholder()
            End If
            MyBase.OnLostFocus(e)
        End Sub

        Public Overrides Property Text As String
            Get
                Return If(_isPlaceholderActive, String.Empty, MyBase.Text)
            End Get
            Set(value As String)
                If String.IsNullOrEmpty(value) Then
                    ShowPlaceholder()
                Else
                    HidePlaceholder()
                    MyBase.Text = value
                End If
            End Set
        End Property
    End Class

    Private Sub btnUploadImage_Click(sender As Object, e As EventArgs) Handles btnUploadImage.Click
        Try
            If OpenFileDialog1 Is Nothing Then
                OpenFileDialog1 = New OpenFileDialog()
            End If

            OpenFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            OpenFileDialog1.Title = "Select Package Image"

            If OpenFileDialog1.ShowDialog() = DialogResult.OK Then
                ' Store the full path
                selectedImagePath = OpenFileDialog1.FileName

                ' Get filename WITHOUT extension - THIS IS THE KEY FIX
                Dim fullFileName As String = Path.GetFileName(selectedImagePath)
                currentImageFileName = Path.GetFileNameWithoutExtension(selectedImagePath)

                ' Load image safely
                Try
                    If PictureBoxImage.Image IsNot Nothing Then
                        PictureBoxImage.Image.Dispose()
                    End If

                    Dim imageBytes As Byte() = File.ReadAllBytes(selectedImagePath)
                    Using ms As New MemoryStream(imageBytes)
                        Dim originalImage As Image = Image.FromStream(ms)
                        Dim imageCopy As New Bitmap(originalImage)
                        originalImage.Dispose()
                        PictureBoxImage.Image = imageCopy
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error loading image: {ex.Message}")
                    Return
                End Try

                ' Try to set txtImageName if it exists
                Try
                    Dim txtControl = FindControl(Me, "txtImageName")
                    If txtControl IsNot Nothing AndAlso TypeOf txtControl Is TextBox Then
                        DirectCast(txtControl, TextBox).Text = currentImageFileName
                    End If
                Catch
                    ' Ignore if txtImageName doesn't exist
                End Try

                MessageBox.Show($"Image uploaded! Stored filename: '{currentImageFileName}'")
            End If
        Catch ex As Exception
            MessageBox.Show($"Error: {ex.Message}")
        End Try
    End Sub

    ' ADD THIS HELPER FUNCTION:
    Private Function FindControl(parent As Control, controlName As String) As Control
        For Each ctrl As Control In parent.Controls
            If ctrl.Name = controlName Then
                Return ctrl
            End If

            Dim found As Control = FindControl(ctrl, controlName)
            If found IsNot Nothing Then
                Return found
            End If
        Next
        Return Nothing
    End Function

    Private Function SavePackageImage(packageId As Integer) As Boolean
        Try
            If PictureBoxImage.Image Is Nothing Then
                MessageBox.Show("No image to save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If

            If String.IsNullOrEmpty(selectedImagePath) Then
                MessageBox.Show("No image file selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If

            ' Always use the original filename without packageId prefix
            Dim originalFileName As String = Path.GetFileName(selectedImagePath)
            Dim finalFileName As String = originalFileName
            Dim extension As String = Path.GetExtension(selectedImagePath)

            ' Get Resources path
            Dim resourcesPath As String = Path.GetFullPath(Path.Combine(Application.StartupPath, "..\..\Resources"))
            If Not Directory.Exists(resourcesPath) Then
                Directory.CreateDirectory(resourcesPath)
            End If

            Dim savedPath As String = Path.Combine(resourcesPath, finalFileName)

            ' Save the image
            Try
                If File.Exists(savedPath) Then
                    File.Delete(savedPath)
                End If

                ' Determine format
                Dim format As Imaging.ImageFormat
                Select Case extension.ToLower()
                    Case ".png"
                        format = Imaging.ImageFormat.Png
                    Case ".gif"
                        format = Imaging.ImageFormat.Gif
                    Case ".bmp"
                        format = Imaging.ImageFormat.Bmp
                    Case Else
                        format = Imaging.ImageFormat.Jpeg
                End Select

                PictureBoxImage.Image.Save(savedPath, format)

                ' Update database with the final filename
                Dim updateImageQuery = "UPDATE PackageDetails SET ImageFileName = @ImageFileName WHERE PackageID = @PackageID"
                Using cmd As New MySqlCommand(updateImageQuery, connection)
                    cmd.Parameters.AddWithValue("@ImageFileName", originalFileName)
                    cmd.Parameters.AddWithValue("@PackageID", packageId)
                    cmd.ExecuteNonQuery()
                End Using

                Return True

            Catch ex As Exception
                MessageBox.Show($"Failed to save image: {ex.Message}")
                Return False
            End Try

        Catch ex As Exception
            MessageBox.Show($"Error: {ex.Message}")
            Return False
        End Try
    End Function

    ' Package details components
    Private Sub CreatePackageDetailLabel(text As String, yPos As Integer)
        Dim lbl As New Label()
        lbl.Text = text
        lbl.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        lbl.ForeColor = Color.FromArgb(75, 85, 99)
        lbl.Location = New Point(15, yPos)
        lbl.Size = New Size(250, 20)
        lbl.AutoSize = False
        packageDetailsGroupBox.Controls.Add(lbl)
    End Sub

    Private Sub LoadCategories()
        Try
            ' Ensure connection is open
            If connection Is Nothing Then
                connection = New MySqlConnection(connectionString)
            End If

            If connection.State <> ConnectionState.Closed Then
                connection.Open()
            End If

            Dim query = "SELECT CategoryID, CategoryName, CategoryType FROM TourCategories WHERE IsActive = 1 ORDER BY CategoryType, CategoryName"
            Dim adapter As New MySql.Data.MySqlClient.MySqlDataAdapter(query, connection)
            Dim dataTable As New DataTable()
            adapter.Fill(dataTable)

            newPackageCategoryComboBox.DisplayMember = "CategoryName"
            newPackageCategoryComboBox.ValueMember = "CategoryID"
            newPackageCategoryComboBox.DataSource = dataTable

            AddHandler newPackageCategoryComboBox.SelectedIndexChanged, AddressOf CategoryComboBox_SelectedIndexChanged

        Catch ex As Exception
            MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub CategoryComboBox_SelectedIndexChanged(sender As Object, e As EventArgs)
        If newPackageCategoryComboBox.SelectedValue IsNot Nothing Then
            Dim selectedRow = CType(newPackageCategoryComboBox.SelectedItem, DataRowView)
            packageTypeComboBox.SelectedItem = selectedRow("CategoryType").ToString()
        End If
    End Sub

    Private Function CreatePackageDetailTextBox(yPos As Integer) As TextBox
        Dim txt As New TextBox()
        txt.Location = New Point(15, yPos)
        txt.Size = New Size(330, 25)
        txt.Font = New Font("Segoe UI", 9)
        Return txt
    End Function

    ' Create New Package Controls
    Private Sub CreateNewPackageButton_Click(sender As Object, e As EventArgs)
        currentView = "newpackage"
        ShowNewPackageCreation()
        HighlightActiveButton(createNewPackageButton)
        ClearNewPackageDetails()
    End Sub

    Private Sub CreateNewPackageDetailLabel(text As String, yPos As Integer)
        Dim lbl As New Label()
        lbl.Text = text
        lbl.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        lbl.ForeColor = Color.FromArgb(75, 85, 99)
        lbl.Location = New Point(15, yPos)
        lbl.Size = New Size(300, 20)
        lbl.AutoSize = False
        newPackageScrollPanel.Controls.Add(lbl)
    End Sub

    Private Function CreateNewPackageDetailTextBox(yPos As Integer) As TextBox
        Dim txt As New TextBox()
        txt.Location = New Point(15, yPos)
        txt.Size = New Size(300, 25)
        txt.Font = New Font("Segoe UI", 9)
        Return txt
    End Function

    Private Sub ShowNewPackageCreation()
        ' Hide other controls
        bookingsDataGrid.Visible = False
        detailsGroupBox.Visible = False
        packagesDataGrid.Visible = False
        packageDetailsGroupBox.Visible = False

        ' Show new package controls
        newPackageDetailsGroupBox.Visible = True

        ' Show details panel and restore normal padding
        detailsPanel.Visible = True
        gridPanel.Padding = New Padding(270, 95, 470, 60)
    End Sub

    Private Sub CreateCompletePackageButton_Click(sender As Object, e As EventArgs) Handles createPackageButton.Click
        Try
            ' Validate required fields
            If newPackageCategoryComboBox.SelectedValue Is Nothing OrElse
       String.IsNullOrWhiteSpace(packageTitleTextBox.Text) OrElse
       String.IsNullOrWhiteSpace(packageLocationTextBox.Text) OrElse
       String.IsNullOrWhiteSpace(packagePriceTextBox.Text) OrElse
       String.IsNullOrWhiteSpace(packageDescriptionTextBox.Text) OrElse
       packageTypeComboBox.SelectedItem Is Nothing Then
                MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Validate image was uploaded
            If PictureBoxImage.Image Is Nothing OrElse String.IsNullOrWhiteSpace(currentImageFileName) Then
                MessageBox.Show("Please upload a package image.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Validate price is numeric
            Dim price As Decimal
            If Not Decimal.TryParse(packagePriceTextBox.Text, price) Then
                MessageBox.Show("Please enter a valid price.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Using transaction = connection.BeginTransaction()
                Try
                    ' Insert tour package (without stored totals)
                    Dim insertPackageQuery = "
            INSERT INTO TourPackages 
            (Title, PackageType, Price, Description, Duration, DurationNights, 
             CategoryID, MaxSlots, Status, StartDate, EndDate, IsGroupPackage, GroupSize, IsActive)
            VALUES (@Title, @PackageType, @Price, @Description, @Duration, @DurationNights,
                    @CategoryID, @MaxSlots, 'Active', @StartDate, @EndDate, 1, @GroupSize, 1);
            SELECT LAST_INSERT_ID();"

                    Dim packageId As Integer
                    Using cmd As New MySqlCommand(insertPackageQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@Title", packageTitleTextBox.Text)
                        cmd.Parameters.AddWithValue("@PackageType", packageTypeComboBox.SelectedItem.ToString())
                        cmd.Parameters.AddWithValue("@Price", price)
                        cmd.Parameters.AddWithValue("@Description", packageDescriptionTextBox.Text)
                        cmd.Parameters.AddWithValue("@Duration", durationNumeric.Value)
                        cmd.Parameters.AddWithValue("@DurationNights", Math.Max(0, durationNumeric.Value - 1))
                        cmd.Parameters.AddWithValue("@CategoryID", newPackageCategoryComboBox.SelectedValue)
                        cmd.Parameters.AddWithValue("@MaxSlots", packageSlotsNumeric.Value)
                        cmd.Parameters.AddWithValue("@StartDate", DateTime.Now.Date)
                        cmd.Parameters.AddWithValue("@EndDate", DateTime.Now.Date.AddDays(durationNumeric.Value))
                        cmd.Parameters.AddWithValue("@GroupSize", packageSlotsNumeric.Value)

                        packageId = Convert.ToInt32(cmd.ExecuteScalar())
                    End Using

                    ' Insert package location
                    Dim insertLocationQuery = "INSERT INTO PackageLocations (PackageID, Location) VALUES (@PackageID, @Location)"
                    Using cmd As New MySqlCommand(insertLocationQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@PackageID", packageId)
                        cmd.Parameters.AddWithValue("@Location", packageLocationTextBox.Text)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Save the uploaded image
                    Dim imageSaved As Boolean = SavePackageImage(packageId)
                    If Not imageSaved Then
                        transaction.Rollback()
                        Return
                    End If

                    ' Insert package details with image filename
                    Dim insertDetailsQuery = "
            INSERT INTO PackageDetails 
            (PackageID, PackageType, FlagSymbol, IdealFor, Inclusions, Highlights, ImageFileName)
            VALUES (@PackageID, @PackageType, @FlagSymbol, @IdealFor, @Inclusions, @Highlights, @ImageFileName)"

                    Using cmd As New MySqlCommand(insertDetailsQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@PackageID", packageId)
                        cmd.Parameters.AddWithValue("@PackageType", packageTypeComboBox.SelectedItem.ToString())
                        cmd.Parameters.AddWithValue("@FlagSymbol", newPackageFlagTextBox.Text)
                        cmd.Parameters.AddWithValue("@IdealFor", newPackageIdealForTextBox.Text)
                        cmd.Parameters.AddWithValue("@Inclusions", newPackageInclusionsTextBox.Text)
                        cmd.Parameters.AddWithValue("@Highlights", newPackageHighlightsTextBox.Text)
                        cmd.Parameters.AddWithValue("@ImageFileName", Path.GetFileName(selectedImagePath))
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Insert addons if provided
                    If Not String.IsNullOrWhiteSpace(packageAddonsTextBox.Text) Then
                        Dim addons = packageAddonsTextBox.Text.Split(","c)
                        For Each addon In addons
                            If Not String.IsNullOrWhiteSpace(addon.Trim()) Then
                                Dim insertAddonQuery = "
                        INSERT INTO PackageAddOns 
                        (PackageID, AddOnName, Price, Unit, IsActive)
                        VALUES (@PackageID, @AddOnName, 0, 'per person', 1)"

                                Using cmd As New MySqlCommand(insertAddonQuery, connection, transaction)
                                    cmd.Parameters.AddWithValue("@PackageID", packageId)
                                    cmd.Parameters.AddWithValue("@AddOnName", addon.Trim())
                                    cmd.ExecuteNonQuery()
                                End Using
                            End If
                        Next
                    End If

                    transaction.Commit()
                    MessageBox.Show("Complete package created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    ClearNewPackageDetails()

                    ' Generate the package card with image
                    GenerateAndShowNewPackageCard(packageId)

                Catch ex As Exception
                    transaction.Rollback()
                    MessageBox.Show($"Error creating package: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End Using

        Catch ex As Exception
            MessageBox.Show($"Error creating package: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    Private Sub ResetNewPackageButton_Click(sender As Object, e As EventArgs)
        ClearNewPackageDetails()
    End Sub

    Private Sub ClearNewPackageDetails()
        newPackageCategoryComboBox.SelectedIndex = -1
        packageTitleTextBox.Clear()
        packageLocationTextBox.Clear()
        packageTypeComboBox.SelectedIndex = -1
        packagePriceTextBox.Clear()
        durationNumeric.Value = 1
        packageSlotsNumeric.Value = 1
        newPackageFlagTextBox.Clear()
        newPackageIdealForTextBox.Clear()
        packageDescriptionTextBox.Clear()
        newPackageInclusionsTextBox.Clear()
        newPackageHighlightsTextBox.Clear()
    End Sub

    Private Sub CreateCompleteNewPackage()
        Try
            ' Validate input
            If newPackageCategoryComboBox.SelectedValue Is Nothing OrElse
           String.IsNullOrWhiteSpace(packageTitleTextBox.Text) OrElse
           String.IsNullOrWhiteSpace(packageLocationTextBox.Text) OrElse
           String.IsNullOrWhiteSpace(packagePriceTextBox.Text) OrElse
           String.IsNullOrWhiteSpace(packageDescriptionTextBox.Text) OrElse
           packageTypeComboBox.SelectedItem Is Nothing Then
                MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim price As Decimal
            If Not Decimal.TryParse(packagePriceTextBox.Text, price) Then
                MessageBox.Show("Please enter a valid price.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Get selected package type from combobox
            Dim selectedPackageType As String = packageTypeComboBox.SelectedItem.ToString()

            Using transaction = connection.BeginTransaction()
                Try
                    ' Insert tour package (without stored totals)
                    Dim insertPackageQuery = "
                    INSERT INTO TourPackages 
                    (Title, PackageType, Price, Description, Duration, DurationNights, 
                     CategoryID, MaxSlots, Status, StartDate, EndDate, IsGroupPackage, GroupSize, IsActive)
                    VALUES (@Title, @PackageType, @Price, @Description, @Duration, @DurationNights,
                            @CategoryID, @MaxSlots, 'Active', @StartDate, @EndDate, 1, @GroupSize, 1);
                    SELECT LAST_INSERT_ID();"

                    Dim packageId As Integer
                    Using cmd As New MySqlCommand(insertPackageQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@Title", packageTitleTextBox.Text)
                        cmd.Parameters.AddWithValue("@PackageType", selectedPackageType)
                        cmd.Parameters.AddWithValue("@Price", price)
                        cmd.Parameters.AddWithValue("@Description", packageDescriptionTextBox.Text)
                        cmd.Parameters.AddWithValue("@Duration", durationNumeric.Value)
                        cmd.Parameters.AddWithValue("@DurationNights", Math.Max(0, durationNumeric.Value - 1))
                        cmd.Parameters.AddWithValue("@CategoryID", newPackageCategoryComboBox.SelectedValue)
                        cmd.Parameters.AddWithValue("@MaxSlots", packageSlotsNumeric.Value)
                        cmd.Parameters.AddWithValue("@StartDate", packageStartDatePicker.Value.Date)
                        cmd.Parameters.AddWithValue("@EndDate", packageEndDatePicker.Value.Date)
                        cmd.Parameters.AddWithValue("@GroupSize", packageSlotsNumeric.Value)

                        packageId = Convert.ToInt32(cmd.ExecuteScalar())
                    End Using

                    ' Insert package location
                    Dim insertLocationQuery = "INSERT INTO PackageLocations (PackageID, Location) VALUES (@PackageID, @Location)"
                    Using cmd As New MySqlCommand(insertLocationQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@PackageID", packageId)
                        cmd.Parameters.AddWithValue("@Location", packageLocationTextBox.Text)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Insert package details
                    Dim insertDetailsQuery = "
                    INSERT INTO PackageDetails 
                    (PackageID, PackageType, FlagSymbol, IdealFor, Inclusions, Highlights, ImageFileName)
                    VALUES (@PackageID, @PackageType, @FlagSymbol, @IdealFor, @Inclusions, @Highlights, @ImageFileName)"

                    Using cmd As New MySqlCommand(insertDetailsQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@PackageID", packageId)
                        cmd.Parameters.AddWithValue("@PackageType", selectedPackageType)
                        cmd.Parameters.AddWithValue("@FlagSymbol", newPackageFlagTextBox.Text)
                        cmd.Parameters.AddWithValue("@IdealFor", newPackageIdealForTextBox.Text)
                        cmd.Parameters.AddWithValue("@Inclusions", newPackageInclusionsTextBox.Text)
                        cmd.Parameters.AddWithValue("@Highlights", newPackageHighlightsTextBox.Text)
                        cmd.Parameters.AddWithValue("@ImageFileName", $"{packageId}_{selectedPackageType.ToLower()}.jpg")
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Insert addons if provided
                    If Not String.IsNullOrWhiteSpace(packageAddonsTextBox.Text) Then
                        Dim addons = packageAddonsTextBox.Text.Split(","c)
                        For Each addon In addons
                            If Not String.IsNullOrWhiteSpace(addon.Trim()) Then
                                Dim insertAddonQuery = "
                                INSERT INTO PackageAddOns 
                                (PackageID, AddOnName, Price, Unit, IsActive)
                                VALUES (@PackageID, @AddOnName, 0, 'per person', 1)"

                                Using cmd As New MySqlCommand(insertAddonQuery, connection, transaction)
                                    cmd.Parameters.AddWithValue("@PackageID", packageId)
                                    cmd.Parameters.AddWithValue("@AddOnName", addon.Trim())
                                    cmd.ExecuteNonQuery()
                                End Using
                            End If
                        Next
                    End If

                    transaction.Commit()
                    MessageBox.Show("Complete package created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    ClearNewPackageDetails()

                    ' Refresh package display
                    If currentView = "cards" Then

                    Else
                        LoadPackagesData()
                    End If

                Catch ex As Exception
                    transaction.Rollback()
                    MessageBox.Show($"Error creating package: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End Using

        Catch ex As Exception
            MessageBox.Show($"Error creating package: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub GenerateAndShowNewPackageCard(packageId As Integer)
        Try
            ' Switch to cards view
            currentView = "cards"
            HighlightActiveButton(packageManagementButton)

            ' Load the newly created package data
            Dim query = "
        SELECT 
            tp.PackageID,
            tp.Title,
            pl.Location,
            tp.Price,
            tp.Duration,
            pd.FlagSymbol,
            pd.IdealFor,
            pd.Inclusions,
            pd.Highlights,
            pd.ImageFileName,
            tp.PackageType,
            (SELECT COUNT(*) FROM Reviews WHERE r.PackageID = tp.PackageID) AS ReviewCount,
            (SELECT COALESCE(AVG(Rating), 0) FROM Reviews WHERE r.PackageID = tp.PackageID) AS Rating,
            (tp.MaxSlots - COALESCE((SELECT SUM(NumberOfPeople) FROM Bookings WHERE b.PackageID = tp.PackageID AND Status IN ('Pending', 'Confirmed')), 0)) AS AvailableSlots
        FROM TourPackages tp
        LEFT JOIN PackageLocations pl ON tp.PackageID = pl.PackageID
        LEFT JOIN PackageDetails pd ON tp.PackageID = pd.PackageID
        LEFT JOIN Reviews r ON tp.PackageID = r.PackageID
        LEFT JOIN Bookings b ON tp.PackageID = b.PackageID  
        WHERE tp.PackageID = @PackageID"

            Dim package As New TravelPackage()
            Using cmd As New MySqlCommand(query, connection)
                cmd.Parameters.AddWithValue("@PackageID", packageId)
                Using reader = cmd.ExecuteReader()
                    If reader.Read() Then
                        package.PackageID = Convert.ToInt32(reader("PackageID"))
                        package.Name = reader("Title").ToString()
                        package.Destination = reader("Location").ToString()
                        package.Price = Convert.ToDecimal(reader("Price"))
                        package.Duration = $"{reader("Duration")} days"
                        package.Flag = If(reader("FlagSymbol") IsNot DBNull.Value, reader("FlagSymbol").ToString(), "🏝️")
                        package.IdealFor = If(reader("IdealFor") IsNot DBNull.Value, reader("IdealFor").ToString(), "Everyone")
                        package.ImageFileName = If(reader("ImageFileName") IsNot DBNull.Value, reader("ImageFileName").ToString(), "")
                        package.Rating = If(reader("Rating") IsNot DBNull.Value, Convert.ToDouble(reader("Rating")), 0)
                        package.ReviewCount = If(reader("ReviewCount") IsNot DBNull.Value, Convert.ToInt32(reader("ReviewCount")), 0)
                        package.AvailableSlots = If(reader("AvailableSlots") IsNot DBNull.Value, Convert.ToInt32(reader("AvailableSlots")), 0)

                        ' Parse inclusions and highlights
                        If reader("Inclusions") IsNot DBNull.Value AndAlso Not String.IsNullOrEmpty(reader("Inclusions").ToString()) Then
                            package.Inclusions = reader("Inclusions").ToString().Split(vbNewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList()
                        End If

                        If reader("Highlights") IsNot DBNull.Value AndAlso Not String.IsNullOrEmpty(reader("Highlights").ToString()) Then
                            package.Highlights = reader("Highlights").ToString().Split(vbNewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList()
                        End If
                    End If
                End Using
            End Using

            ' Create and display the card
            Dim card = CreatePackageCard(package)
            card.Location = New Point(20, 20) ' Position at top of scrollable area

            ' Clear existing cards and add the new one

            ' Show success message
            MessageBox.Show($"Package '{package.Name}' created successfully and displayed in the Package Cards view.",
                       "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show($"Error displaying new package card: {ex.Message}", "Display Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Booking details components
    Private Sub CreateDetailLabel(text As String, yPos As Integer)
        Dim lbl As New Label()
        lbl.Text = text
        lbl.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        lbl.ForeColor = Color.FromArgb(75, 85, 99)
        lbl.Location = New Point(15, yPos)
        lbl.Size = New Size(250, 20)
        lbl.AutoSize = False
        detailsGroupBox.Controls.Add(lbl)
    End Sub

    Private Function CreateDetailTextBox(yPos As Integer) As TextBox
        Dim txt As New TextBox()
        txt.Location = New Point(15, yPos)
        txt.Size = New Size(330, 25)
        txt.Font = New Font("Segoe UI", 9)
        Return txt
    End Function

    Private Sub InitializeDatabase()
        Try
            If connection Is Nothing Then
                connection = New MySqlConnection(connectionString)
            End If

            If connection.State <> ConnectionState.Open Then
                connection.Open()

                ' Create views if they don't exist
                Dim createViewsQuery As String = "
                CREATE OR REPLACE VIEW BookingAmounts AS
                SELECT 
                    b.BookingID,
                    b.BookingReference,
                    b.UserID,
                    b.PackageID,
                    b.NumberOfPeople,
                    b.PromoID,
                    (tp.Price * b.NumberOfPeople) AS BaseAmount,
                    COALESCE(addon_totals.AddOnTotal, 0) AS AddOnTotal,
                    ((tp.Price * b.NumberOfPeople) + COALESCE(addon_totals.AddOnTotal, 0)) AS SubTotal,
                    CASE 
                        WHEN b.PromoID IS NOT NULL AND pd.Status = 'Approved' THEN
                            ROUND(((tp.Price * b.NumberOfPeople) + COALESCE(addon_totals.AddOnTotal, 0)) * (pt.DiscountPercentage / 100), 2)
                        ELSE 0
                    END AS DiscountAmount,
                    ((tp.Price * b.NumberOfPeople) + COALESCE(addon_totals.AddOnTotal, 0)) - 
                    CASE 
                        WHEN b.PromoID IS NOT NULL AND pd.Status = 'Approved' THEN
                            ROUND(((tp.Price * b.NumberOfPeople) + COALESCE(addon_totals.AddOnTotal, 0)) * (pt.DiscountPercentage / 100), 2)
                        ELSE 0
                    END AS FinalAmount,
                    pt.PromoTypeName,
                    pt.DiscountPercentage
                FROM Bookings b
                JOIN TourPackages tp ON b.PackageID = tp.PackageID
                LEFT JOIN PromoDiscounts pd ON b.PromoID = pd.PromoID
                LEFT JOIN PromoTypes pt ON pd.PromoTypeID = pt.PromoTypeID
                LEFT JOIN (
                    SELECT 
                        BookingID,
                        SUM(UnitPrice * Quantity) AS AddOnTotal
                    FROM BookingAddOns
                    GROUP BY BookingID
                ) addon_totals ON b.BookingID = addon_totals.BookingID;

                CREATE OR REPLACE VIEW PackageAvailability AS
                SELECT 
                    tp.PackageID,
                    tp.Title,
                    tp.PackageType,
                    tp.MaxSlots,
                    COALESCE(booked_slots.BookedSlots, 0) AS BookedSlots,
                    (tp.MaxSlots - COALESCE(booked_slots.BookedSlots, 0)) AS AvailableSlots,
                    tp.StartDate,
                    tp.EndDate,
                    tp.Status,
                    tp.IsActive
                FROM TourPackages tp
                LEFT JOIN (
                    SELECT 
                        PackageID,
                        SUM(NumberOfPeople) AS BookedSlots
                    FROM Bookings
                    WHERE Status IN ('Pending', 'Confirmed')
                    GROUP BY PackageID
                ) booked_slots ON tp.PackageID = booked_slots.PackageID;"

                Using cmd As New MySqlCommand(createViewsQuery, connection)
                    cmd.ExecuteNonQuery()
                End Using
            End If
        Catch ex As Exception
            MessageBox.Show($"Database initialization failed: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    'Load Bookings
    Private Sub LoadBookingsData(Optional whereClause As String = "")
        Try
            Dim query As String = "
        SELECT 
            b.BookingID,
            b.BookingReference,
            CONCAT(u.FirstName, ' ', u.LastName) AS CustomerName,
            b.Status,
            COALESCE(bp.PaymentStatus, 'Pending') AS PaymentStatus,
            b.BookingDate,
            ba.FinalAmount,
            tp.Title AS PackageTitle,
            pl.Location AS PackageLocation
        FROM Bookings b
        JOIN Users u ON b.UserID = u.UserID
        LEFT JOIN BookingPayments bp ON b.BookingID = bp.BookingID
        LEFT JOIN TourPackages tp ON b.PackageID = tp.PackageID
        LEFT JOIN PackageLocations pl ON tp.PackageID = pl.PackageID
        LEFT JOIN BookingAmounts ba ON b.BookingID = ba.BookingID"

            If Not String.IsNullOrEmpty(whereClause) Then
                query += " WHERE " + whereClause
            End If

            query += " ORDER BY b.BookingDate DESC"

            Dim adapter As New MySql.Data.MySqlClient.MySqlDataAdapter(query, connection)
            Dim dataTable As New DataTable()
            adapter.Fill(dataTable)

            ' Temporarily disable binding events
            RemoveHandler bookingsDataGrid.SelectionChanged, AddressOf BookingsDataGrid_SelectionChanged
            bookingsDataGrid.DataSource = dataTable
            AddHandler bookingsDataGrid.SelectionChanged, AddressOf BookingsDataGrid_SelectionChanged

            ' Format columns
            FormatDataGridColumns()

            ' Apply cell formatting
            ApplyPaymentStatusFormatting()

        Catch ex As Exception
            MessageBox.Show($"Error loading bookings: {ex.Message}", "Data Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub ApplyPaymentStatusFormatting()
        If bookingsDataGrid.Columns.Contains("PaymentStatus") Then
            For Each row As DataGridViewRow In bookingsDataGrid.Rows
                If Not row.IsNewRow AndAlso row.Cells("PaymentStatus").Value IsNot Nothing Then
                    Dim status = row.Cells("PaymentStatus").Value.ToString()
                    Select Case status
                        Case "Paid"
                            row.Cells("PaymentStatus").Style.BackColor = Color.FromArgb(220, 252, 231)
                            row.Cells("PaymentStatus").Style.ForeColor = Color.FromArgb(22, 163, 74)
                        Case "Pending"
                            row.Cells("PaymentStatus").Style.BackColor = Color.FromArgb(254, 249, 195)
                            row.Cells("PaymentStatus").Style.ForeColor = Color.FromArgb(180, 83, 9)
                        Case "Cancelled"
                            row.Cells("PaymentStatus").Style.BackColor = Color.FromArgb(254, 226, 226)
                            row.Cells("PaymentStatus").Style.ForeColor = Color.FromArgb(220, 38, 38)
                    End Select
                End If
            Next
        End If
    End Sub


    ' Bookings DataGrid Formatting
    Private Sub FormatDataGridColumns()
        If bookingsDataGrid.Columns.Count = 0 Then Return

        ' Set all columns to not autosize
        bookingsDataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None

        ' Create a list of columns we want to keep visible with specific column names
        Dim visibleColumns As New Dictionary(Of String, (HeaderText As String, Width As Integer)) From {
                {"BookingID", ("Booking ID", 70)},
                {"UserID", ("User ID", 65)},
                {"CustomerName", ("Customer Name", 140)},
                {"BookingReference", ("Booking Reference", 120)},
                {"Status", ("Status", 80)},
                {"PaymentStatus", ("Payment Status", 100)},
                {"BookingDate", ("Booking Date", 100)}
            }

        ' Configure columns
        For Each col As DataGridViewColumn In bookingsDataGrid.Columns
            If visibleColumns.ContainsKey(col.Name) Then
                ' Configure visible columns
                col.HeaderText = visibleColumns(col.Name).HeaderText
                col.Width = visibleColumns(col.Name).Width
                col.Visible = True
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
                ' Make columns unresizable
                col.Resizable = DataGridViewTriState.False
            Else
                ' Hide other columns
                col.Visible = False
            End If
        Next

        ' Format date column if it exists
        If bookingsDataGrid.Columns.Contains("BookingDate") Then
            bookingsDataGrid.Columns("BookingDate").DefaultCellStyle.Format = "MM/dd/yyyy"
        End If
    End Sub

    Private Sub SetColumnProperties(columnName As String, headerText As String, width As Integer, visible As Boolean)
        If bookingsDataGrid.Columns.Contains(columnName) Then
            With bookingsDataGrid.Columns(columnName)
                .HeaderText = headerText
                .Width = width
                .Visible = visible
                .HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
                .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
                .Resizable = DataGridViewTriState.False
            End With
        End If
    End Sub
    Private Sub BookingsDataGrid_SelectionChanged(sender As Object, e As EventArgs)
        If bookingsDataGrid.SelectedRows.Count > 0 Then
            LoadBookingDetails()
        End If
    End Sub

    ' Load Booking Details
    Private Sub LoadBookingDetails()
        Try
            If bookingsDataGrid.SelectedRows.Count = 0 Then Return

            Dim bookingId = Convert.ToInt32(bookingsDataGrid.SelectedRows(0).Cells("BookingID").Value)

            Dim query As String = "
        SELECT 
            b.BookingID,
            b.BookingReference,
            CONCAT(u.FirstName, ' ', u.LastName) AS CustomerName,
            u.Email,
            u.Phone,
            b.Status,
            COALESCE(bp.PaymentStatus, 'Pending') AS PaymentStatus,
            COALESCE(bp.PaymentMethod, 'Not specified') AS PaymentMethod,
            b.TravelDate,
            b.EndDate,
            b.NumberOfPeople,
            ba.FinalAmount,
            tp.Title AS PackageTitle,
            pl.Location AS PackageLocation,
            tp.PackageType
        FROM Bookings b
        JOIN Users u ON b.UserID = u.UserID
        LEFT JOIN BookingPayments bp ON b.BookingID = bp.BookingID
        LEFT JOIN TourPackages tp ON b.PackageID = tp.PackageID
        LEFT JOIN PackageLocations pl ON tp.PackageID = pl.PackageID
        LEFT JOIN BookingAmounts ba ON b.BookingID = ba.BookingID
        WHERE b.BookingID = @BookingID"

            Using cmd As New MySqlCommand(query, connection)
                cmd.Parameters.AddWithValue("@BookingID", bookingId)
                Using reader = cmd.ExecuteReader()
                    If reader.Read() Then
                        bookingIdLabel.Text = $"Booking ID: {reader("BookingID")}"
                        bookingReferenceLabel.Text = $"Reference: {reader("BookingReference")}"
                        customerNameTextBox.Text = reader("CustomerName").ToString()
                        customerEmailTextBox.Text = reader("Email").ToString()
                        customerPhoneTextBox.Text = If(reader("Phone") IsNot DBNull.Value, reader("Phone").ToString(), "")

                        packageTitleLabel.Text = $"Package: {reader("PackageTitle")}"
                        packageLocationLabel.Text = $"Location: {reader("PackageLocation")}"
                        packageTypeLabel.Text = $"Type: {reader("PackageType")}"

                        travelDatePicker.Value = Convert.ToDateTime(reader("TravelDate"))
                        endDatePicker.Value = Convert.ToDateTime(reader("EndDate"))
                        numberOfPeopleNumeric.Value = Convert.ToDecimal(reader("NumberOfPeople"))

                        finalAmountTextBox.Text = Convert.ToDecimal(reader("FinalAmount")).ToString("N2")

                        ' Set status and payment status
                        statusComboBox.SelectedItem = reader("Status").ToString()
                        paymentStatusComboBox.SelectedItem = reader("PaymentStatus").ToString()
                        paymentMethodLabel.Text = $"Payment Method: {reader("PaymentMethod").ToString()}"

                        ' Force UI update
                        statusComboBox.Refresh()
                        paymentStatusComboBox.Refresh()
                        paymentMethodLabel.Refresh()
                    End If
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error loading booking details: {ex.Message}", "Detail Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function GetCellValue(row As DataGridViewRow, columnName As String, Optional defaultValue As String = "") As String
        If bookingsDataGrid.Columns.Contains(columnName) AndAlso row.Cells(columnName).Value IsNot Nothing AndAlso Not IsDBNull(row.Cells(columnName).Value) Then
            Return row.Cells(columnName).Value.ToString()
        Else
            Return defaultValue
        End If
    End Function

    ' Dashboard Event Handlers
    Private Sub RefreshButton_Click(sender As Object, e As EventArgs)
        LoadBookingsData()
        ClearBookingDetails()
        MessageBox.Show("Bookings refreshed successfully!", "Refresh Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub LogoutButton_Click(sender As Object, e As EventArgs)
        Dim result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If result = DialogResult.Yes Then
            If connection IsNot Nothing AndAlso connection.State = ConnectionState.Open Then
                connection.Close()
            End If

            Me.Close()
            Dim home As New MainForm()
            home.Show()
        End If
    End Sub

    Private Sub AllBookingsButton_Click(sender As Object, e As EventArgs)
        currentView = "bookings"
        ShowBookingManagement()
        LoadBookingsData()
        HighlightActiveButton(allBookingsButton)
        ClearBookingDetails()
    End Sub


    Private Sub PendingBookingsButton_Click(sender As Object, e As EventArgs)
        currentView = "bookings"
        ShowBookingManagement()
        LoadBookingsData("b.Status = 'Pending'")
        HighlightActiveButton(pendingBookingsButton)
        ClearBookingDetails()
    End Sub

    Private Sub ConfirmedBookingsButton_Click(sender As Object, e As EventArgs)
        currentView = "bookings"
        ShowBookingManagement()
        LoadBookingsData("b.Status = 'Confirmed'")
        HighlightActiveButton(confirmedBookingsButton)
        ClearBookingDetails()
    End Sub

    Private Sub CompletedBookingsButton_Click(sender As Object, e As EventArgs)
        currentView = "bookings"
        ShowBookingManagement()
        LoadBookingsData("b.Status = 'Completed'")
        HighlightActiveButton(completedBookingsButton)
        ClearBookingDetails()
    End Sub

    Private Sub CancelledBookingsButton_Click(sender As Object, e As EventArgs)
        currentView = "bookings"
        ShowBookingManagement()
        LoadBookingsData("b.Status = 'Cancelled'")
        HighlightActiveButton(cancelledBookingsButton)
        ClearBookingDetails()
    End Sub

    ' Finance Management
    Private Sub FinancialManagementButton_Click(sender As Object, e As EventArgs)
        Try
            ' Create a new instance each time the button is clicked
            Dim financeForm As New FinancialManagementForms()

            ' Set the owner to prevent the form from being disposed when the main form closes
            financeForm.Show(Me)

            ' Highlight the button
            HighlightActiveButton(financialManagementButton)

        Catch ex As Exception
            MessageBox.Show("Error opening Financial Management: " & ex.Message & vbCrLf & vbCrLf &
                           "Stack Trace: " & ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub SearchButton_Click(sender As Object, e As EventArgs)
        If currentView = "bookings" Then
            PerformBookingSearch()
        Else
            PerformPackageSearch()
        End If
    End Sub

    Private Sub PerformPackageSearch()
        Try
            Dim searchTerm = searchTextBox.Text.Trim()
            Dim filterType = filterComboBox.SelectedItem.ToString()
            Dim whereClause As String = ""

            If Not String.IsNullOrEmpty(searchTerm) Then
                whereClause = $"(tp.Title LIKE '%{searchTerm}%' OR pl.Location LIKE '%{searchTerm}%')"
            End If

            Select Case filterType
                Case "Active"
                    Dim statusClause = "tp.Status = 'Active'"
                    whereClause = If(String.IsNullOrEmpty(whereClause), statusClause, whereClause + " AND " + statusClause)
                Case "Inactive"
                    Dim statusClause = "tp.Status = 'Inactive'"
                    whereClause = If(String.IsNullOrEmpty(whereClause), statusClause, whereClause + " AND " + statusClause)
                Case "Freediving", "Domestic", "International"
                    If filterType <> "All Packages" Then
                        Dim packageTypeClause = $"tp.PackageType = '{filterType}'"
                        whereClause = If(String.IsNullOrEmpty(whereClause), packageTypeClause, whereClause + " AND " + packageTypeClause)
                    End If
            End Select

            LoadPackagesData(whereClause)
            ClearPackageDetails()

        Catch ex As Exception
            MessageBox.Show($"Error performing search: {ex.Message}", "Search Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub PerformBookingSearch()
        Try
            Dim searchTerm = searchTextBox.Text.Trim()
            Dim filterType = filterComboBox.SelectedItem.ToString()
            Dim whereClause As String = ""

            If Not String.IsNullOrEmpty(searchTerm) Then
                whereClause = $"(CONCAT(u.FirstName, ' ', u.LastName) LIKE '%{searchTerm}%' OR " &
                                 $"b.BookingReference LIKE '%{searchTerm}%')"
            End If

            If filterType <> "All Packages" Then
                Dim packageTypeClause = $"tp.PackageType = '{filterType}'"
                If String.IsNullOrEmpty(whereClause) Then
                    whereClause = packageTypeClause
                Else
                    whereClause += " AND " + packageTypeClause
                End If
            End If

            LoadBookingsData(whereClause)
            ClearBookingDetails()

        Catch ex As Exception
            MessageBox.Show($"Error performing search: {ex.Message}", "Search Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Booking Buttons
    Private Sub UpdateButton_Click(sender As Object, e As EventArgs)
        UpdateBookingRecord()
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As EventArgs)
        LoadBookingDetails()
    End Sub

    Private Sub UpdateBookingRecord()
        Try
            If bookingsDataGrid.SelectedRows.Count = 0 Then
                MessageBox.Show("Please select a booking to update.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim bookingId = Convert.ToInt32(bookingsDataGrid.SelectedRows(0).Cells("BookingID").Value)
            Dim bookingRef = bookingsDataGrid.SelectedRows(0).Cells("BookingReference").Value.ToString()
            Dim newStatus = statusComboBox.SelectedItem.ToString()
            Dim newPaymentStatus = paymentStatusComboBox.SelectedItem.ToString()

            ' Validate required fields
            If String.IsNullOrWhiteSpace(customerNameTextBox.Text) Then
                MessageBox.Show("Customer name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            If statusComboBox.SelectedItem Is Nothing Then
                MessageBox.Show("Please select a booking status.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            If paymentStatusComboBox.SelectedItem Is Nothing Then
                MessageBox.Show("Please select a payment status.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Using transaction = connection.BeginTransaction()
                Try
                    ' Get current status before update
                    Dim currentStatus As String = ""
                    Dim currentPaymentStatus As String = ""

                    ' Get current booking status
                    Dim getStatusQuery = "SELECT Status FROM Bookings WHERE BookingID = @BookingID"
                    Using cmd As New MySqlCommand(getStatusQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@BookingID", bookingId)
                        currentStatus = cmd.ExecuteScalar().ToString()
                    End Using

                    ' Get current payment status
                    Dim getPaymentStatusQuery = "SELECT PaymentStatus FROM BookingPayments WHERE BookingID = @BookingID"
                    Using cmd As New MySqlCommand(getPaymentStatusQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@BookingID", bookingId)
                        Dim result = cmd.ExecuteScalar()
                        currentPaymentStatus = If(result IsNot Nothing AndAlso Not IsDBNull(result), result.ToString(), "Pending")
                    End Using

                    ' Update booking information
                    Dim updateBookingQuery = "UPDATE Bookings 
                                        SET TravelDate = @TravelDate,
                                            EndDate = @EndDate,
                                            NumberOfPeople = @NumberOfPeople,
                                            Status = @Status,
                                            UpdatedAt = NOW()
                                        WHERE BookingID = @BookingID"

                    Using cmd As New MySqlCommand(updateBookingQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@TravelDate", travelDatePicker.Value.Date)
                        cmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date)
                        cmd.Parameters.AddWithValue("@NumberOfPeople", numberOfPeopleNumeric.Value)
                        cmd.Parameters.AddWithValue("@Status", newStatus)
                        cmd.Parameters.AddWithValue("@BookingID", bookingId)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Update or insert payment status
                    Dim paymentUpdateQuery = "INSERT INTO BookingPayments 
                                        (BookingID, PaymentStatus, UpdatedAt)
                                        VALUES (@BookingID, @PaymentStatus, NOW())
                                        ON DUPLICATE KEY UPDATE 
                                            PaymentStatus = VALUES(PaymentStatus),
                                            UpdatedAt = NOW()"

                    Using cmd As New MySqlCommand(paymentUpdateQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@BookingID", bookingId)
                        cmd.Parameters.AddWithValue("@PaymentStatus", newPaymentStatus)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Add to booking history if status changed
                    If newStatus <> currentStatus Then
                        Dim insertHistoryQuery = "INSERT INTO BookingHistory 
                                            (BookingID, UserID, ActionType, ActionDescription, PreviousStatus, NewStatus, ModifiedBy)
                                            SELECT 
                                                @BookingID, 
                                                UserID, 
                                                'Status Update', 
                                                @Description, 
                                                @PreviousStatus,
                                                @NewStatus,
                                                (SELECT UserID FROM Users WHERE Role = 'Admin' LIMIT 1)
                                            FROM Bookings 
                                            WHERE BookingID = @BookingID"

                        Using cmd As New MySqlCommand(insertHistoryQuery, connection, transaction)
                            cmd.Parameters.AddWithValue("@BookingID", bookingId)
                            cmd.Parameters.AddWithValue("@Description", $"Booking status changed from {currentStatus} to {newStatus}")
                            cmd.Parameters.AddWithValue("@PreviousStatus", currentStatus)
                            cmd.Parameters.AddWithValue("@NewStatus", newStatus)
                            cmd.ExecuteNonQuery()
                        End Using
                    End If

                    ' Add payment history if payment status changed
                    If newPaymentStatus <> currentPaymentStatus Then
                        Dim insertPaymentHistoryQuery = "INSERT INTO BookingHistory 
                                                  (BookingID, UserID, ActionType, ActionDescription, PreviousStatus, NewStatus, ModifiedBy)
                                                  SELECT 
                                                      @BookingID, 
                                                      UserID, 
                                                      'Payment Update', 
                                                      @Description, 
                                                      @PreviousStatus,
                                                      @NewStatus,
                                                      (SELECT UserID FROM Users WHERE Role = 'Admin' LIMIT 1)
                                                  FROM Bookings 
                                                  WHERE BookingID = @BookingID"

                        Using cmd As New MySqlCommand(insertPaymentHistoryQuery, connection, transaction)
                            cmd.Parameters.AddWithValue("@BookingID", bookingId)
                            cmd.Parameters.AddWithValue("@Description", $"Payment status changed from {currentPaymentStatus} to {newPaymentStatus}")
                            cmd.Parameters.AddWithValue("@PreviousStatus", currentPaymentStatus)
                            cmd.Parameters.AddWithValue("@NewStatus", newPaymentStatus)
                            cmd.ExecuteNonQuery()
                        End Using
                    End If

                    transaction.Commit()

                    ' Refresh the data grid and details
                    RefreshCurrentView()
                    LoadBookingDetails()

                    MessageBox.Show("Booking updated successfully!", "Update Successful", MessageBoxButtons.OK, MessageBoxIcon.Information)

                Catch ex As Exception
                    transaction.Rollback()
                    MessageBox.Show($"Error updating booking: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End Using

        Catch ex As Exception
            MessageBox.Show($"Error updating booking: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub RefreshCurrentView()
        Try
            ' Determine current filter based on active button
            Dim filter As String = ""

            If pendingBookingsButton.BackColor = Color.FromArgb(37, 99, 235) Then
                filter = "b.Status = 'Pending'"
            ElseIf confirmedBookingsButton.BackColor = Color.FromArgb(37, 99, 235) Then
                filter = "b.Status = 'Confirmed'"
            ElseIf completedBookingsButton.BackColor = Color.FromArgb(37, 99, 235) Then
                filter = "b.Status = 'Completed'"
            ElseIf cancelledBookingsButton.BackColor = Color.FromArgb(37, 99, 235) Then
                filter = "b.Status = 'Cancelled'"
            End If

            ' Reload data with current filter
            LoadBookingsData(filter)

            ' If a booking was selected before refresh, try to reselect it
            If bookingsDataGrid.SelectedRows.Count > 0 Then
                Dim selectedId = Convert.ToInt32(bookingsDataGrid.SelectedRows(0).Cells("BookingID").Value)
                For Each row As DataGridViewRow In bookingsDataGrid.Rows
                    If Convert.ToInt32(row.Cells("BookingID").Value) = selectedId Then
                        row.Selected = True
                        LoadBookingDetails()
                        Exit For
                    End If
                Next
            End If

            ' Force UI update
            bookingsDataGrid.Refresh()
            Application.DoEvents()

        Catch ex As Exception
            MessageBox.Show($"Error refreshing view: {ex.Message}", "Refresh Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    ' Package Management Controls
    Private Sub PackageManagementButton_Click(sender As Object, e As EventArgs)
        currentView = "packages"
        ShowPackageManagement()
        HighlightActiveButton(packageManagementButton)
        LoadPackagesData()
        ClearPackageDetails()
    End Sub

    Private Sub ShowPackageManagement()
        ' Hide booking controls
        bookingsDataGrid.Visible = False
        detailsGroupBox.Visible = False


        ' Show package controls
        packagesDataGrid.Visible = True
        packageDetailsGroupBox.Visible = True
        newPackageDetailsGroupBox.Visible = False

        ' Show details panel and restore normal padding
        detailsPanel.Visible = True
        gridPanel.Padding = New Padding(270, 95, 470, 60)

        ' Update search panel for packages
        filterComboBox.Items.Clear()
        filterComboBox.Items.AddRange({"All Packages", "Active", "Inactive", "Freediving", "Domestic", "International"})
        filterComboBox.SelectedIndex = 0
    End Sub

    Private Sub ShowBookingManagement()
        ' Hide package controls
        packagesDataGrid.Visible = False
        packageDetailsGroupBox.Visible = False
        newPackageDetailsGroupBox.Visible = False


        ' Show booking controls
        bookingsDataGrid.Visible = True
        detailsGroupBox.Visible = True

        ' Show details panel and restore normal padding
        detailsPanel.Visible = True
        gridPanel.Padding = New Padding(270, 95, 470, 60)

        ' Reset search panel for bookings
        filterComboBox.Items.Clear()
        filterComboBox.Items.AddRange({"All Packages", "Freediving", "Domestic", "International"})
        filterComboBox.SelectedIndex = 0
    End Sub

    Public Class TravelPackage
        Public Property PackageID As Integer
        Public Property Name As String
        Public Property Destination As String
        Public Property Duration As String
        Public Property Price As Decimal
        Public Property Rating As Double
        Public Property ReviewCount As Integer
        Public Property Description As String
        Public Property IdealFor As String
        Public Property Flag As String
        Public Property Inclusions As List(Of String)
        Public Property Highlights As List(Of String)
        Public Property AvailableSlots As Integer
        Public Property ImageFileName As String

        Public Sub New()
            Inclusions = New List(Of String)()
            Highlights = New List(Of String)()
        End Sub
    End Class

    ' Method to create package card
    Private Function CreatePackageCard(package As TravelPackage) As Panel
        Dim cardWidth As Integer = 400
        Dim cardHeight As Integer = 400

        ' Main card with white background and rounded corners
        Dim card As New Panel With {
        .Size = New Size(cardWidth, cardHeight),
        .BackColor = Color.FromArgb(200, 255, 255, 255),
        .Tag = package,
        .Cursor = Cursors.Hand
    }
        AddHandler card.Paint, AddressOf Card_Paint

        ' Image panel (top portion)
        Dim imagePanel As New Panel With {
        .Size = New Size(cardWidth, 180),
        .Location = New Point(0, 0),
        .BackColor = Color.FromArgb(6, 41, 55)
    }

        ' Load the image if available
        If Not String.IsNullOrEmpty(package.ImageFileName) Then
            Try
                ' Try multiple possible locations for the image
                Dim imagePaths As New List(Of String) From {
                    Path.Combine(Application.StartupPath, "..\..\Resources", package.ImageFileName),
                    Path.Combine(Application.StartupPath, "Resources", package.ImageFileName),
                    Path.GetFullPath(Path.Combine(Application.StartupPath, "..\..\Resources", package.ImageFileName)),
                    Path.Combine("C:\Users\Mykyla\Source\Repos\proje\Resources", package.ImageFileName)
                }

                Dim imagePath As String = imagePaths.FirstOrDefault(Function(p) File.Exists(p))

                If imagePath IsNot Nothing Then
                    Try
                        Dim img = Image.FromFile(imagePath)
                        Dim pictureBox As New PictureBox With {
                            .Size = New Size(cardWidth, 180),
                            .Image = img,
                            .SizeMode = PictureBoxSizeMode.StretchImage,
                            .Dock = DockStyle.Fill
                        }
                        imagePanel.Controls.Add(pictureBox)
                        MessageBox.Show("Image loaded successfully!", "Success")
                    Catch ex As Exception
                        MessageBox.Show($"Error loading image: {ex.Message}", "Error")
                        CreateImagePlaceholder(imagePanel, package.Name)
                    End Try
                Else
                    MessageBox.Show("No image path found!", "No Image")
                    CreateImagePlaceholder(imagePanel, package.Name)
                End If
            Catch ex As Exception
                CreateImagePlaceholder(imagePanel, package.Name)
            End Try
        Else
            CreateImagePlaceholder(imagePanel, package.Name)
        End If
        card.Controls.Add(imagePanel)

        ' Package name label
        Dim nameLabel As New Label With {
        .Text = package.Name,
        .Font = New Font("Segoe UI", 16, FontStyle.Bold),
        .ForeColor = Color.FromArgb(6, 41, 55),
        .BackColor = Color.Transparent,
        .Location = New Point(20, 190),
        .Size = New Size(360, 45),
        .TextAlign = ContentAlignment.TopLeft
    }

        ' Destination label
        Dim destinationLabel As New Label With {
        .Text = $"📍 {package.Destination}",
        .Font = New Font("Segoe UI", 10, FontStyle.Bold),
        .ForeColor = Color.Black,
        .BackColor = Color.Transparent,
        .Location = New Point(20, 230),
        .Size = New Size(360, 25)
    }

        ' Duration label
        Dim durationLabel As New Label With {
        .Text = $"⏱️ {package.Duration}",
        .Font = New Font("Segoe UI", 10, FontStyle.Bold),
        .ForeColor = Color.Black,
        .BackColor = Color.Transparent,
        .Location = New Point(20, 255),
        .Size = New Size(200, 25)
    }

        ' Price label
        Dim priceLabel As New Label With {
        .Text = "₱" & package.Price.ToString("N2", New CultureInfo("en-US")),
        .Font = New Font("Arial", 20, FontStyle.Bold),
        .ForeColor = Color.FromArgb(34, 197, 94),
        .BackColor = Color.Transparent,
        .Location = New Point(20, 275),
        .Size = New Size(200, 30)
    }

        ' Rating stars
        Dim roundedRating As Double = Math.Round(package.Rating * 2) / 2
        Dim fullStars As Integer = CInt(Math.Floor(roundedRating))
        Dim hasHalfStar As Boolean = (roundedRating Mod 1) >= 0.5

        Dim starsText As String = New String("⭐"c, fullStars) &
        If(hasHalfStar, "½", "") &
        New String("☆"c, 5 - fullStars - If(hasHalfStar, 1, 0))

        Dim ratingLabel As New Label With {
        .Text = $"{starsText} {package.Rating.ToString("N1")}",
        .Font = New Font("Segoe UI", 9, FontStyle.Bold),
        .ForeColor = Color.Orange,
        .BackColor = Color.Transparent,
        .Location = New Point(230, 275),
        .Size = New Size(150, 25)
    }

        ' Review count
        Dim reviewCountLabel As New Label With {
        .Text = If(package.ReviewCount > 0, $"({package.ReviewCount} reviews)", "(No reviews)"),
        .Font = New Font("Segoe UI", 8, FontStyle.Bold),
        .ForeColor = Color.Black,
        .BackColor = Color.Transparent,
        .Location = New Point(230, 295),
        .Size = New Size(150, 15)
    }

        ' Availability
        Dim availabilityLabel As New Label With {
        .Text = If(package.AvailableSlots > 0, $"🎫 {package.AvailableSlots} slots", "❌ Fully booked"),
        .Font = New Font("Segoe UI", 9, FontStyle.Bold),
        .ForeColor = If(package.AvailableSlots > 0, Color.FromArgb(34, 197, 94), Color.FromArgb(239, 68, 68)),
        .BackColor = Color.Transparent,
        .Location = New Point(230, 315),
        .Size = New Size(150, 15)
    }

        ' Per person label
        Dim perPersonLabel As New Label With {
        .Text = "per person",
        .Font = New Font("Segoe UI", 9, FontStyle.Bold),
        .ForeColor = Color.Black,
        .BackColor = Color.Transparent,
        .Location = New Point(20, 305),
        .Size = New Size(100, 20)
    }

        ' Edit button
        Dim editBtn As New Button With {
        .Text = "Edit Package",
        .Size = New Size(180, 40),
        .Location = New Point(25, 330),
        .BackColor = Color.FromArgb(37, 99, 235),
        .ForeColor = Color.White,
        .FlatStyle = FlatStyle.Flat,
        .Font = New Font("Segoe UI", 10, FontStyle.Bold),
        .Cursor = Cursors.Hand,
        .Tag = package
    }
        editBtn.FlatAppearance.BorderSize = 0

        ' Delete button
        Dim deleteBtn As New Button With {
        .Text = "Delete",
        .Size = New Size(160, 40),
        .Location = New Point(215, 330),
        .BackColor = Color.FromArgb(239, 68, 68),
        .ForeColor = Color.White,
        .FlatStyle = FlatStyle.Flat,
        .Font = New Font("Segoe UI", 10, FontStyle.Bold),
        .Cursor = Cursors.Hand,
        .Tag = package
    }
        deleteBtn.FlatAppearance.BorderSize = 0

        ' Add event handlers
        AddHandler editBtn.Click, AddressOf EditPackage_Click
        AddHandler deleteBtn.Click, AddressOf DeletePackageFromCard_Click

        ' Add all controls to the card
        card.Controls.AddRange({
        nameLabel, destinationLabel, durationLabel,
        priceLabel, ratingLabel, reviewCountLabel,
        availabilityLabel, perPersonLabel,
        editBtn, deleteBtn
    })

        Return card
    End Function


    ' Paint method for rounded corners
    Private Sub Card_Paint(sender As Object, e As PaintEventArgs)
        Dim panel As Panel = DirectCast(sender, Panel)
        Dim rect As New Rectangle(0, 0, panel.Width, panel.Height)
        Dim radius As Integer = 15

        Using path As New Drawing2D.GraphicsPath()
            path.StartFigure()
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90)
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90)
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90)
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90)
            path.CloseFigure()

            panel.Region = New Region(path)
            e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            e.Graphics.FillPath(New SolidBrush(panel.BackColor), path)
        End Using
    End Sub

    Private Sub CreateImagePlaceholder(panel As Panel, packageName As String)
        ' Background color
        panel.BackColor = Color.FromArgb(6, 41, 55)

        ' Draw package name
        Using g As Graphics = panel.CreateGraphics()
            Using font As New Font("Arial", 14, FontStyle.Bold)
                Using textBrush As New SolidBrush(Color.White)
                    ' Adjust font size if needed
                    Dim adjustedFont As Font = font
                    Dim textSize As SizeF = g.MeasureString(packageName, font)

                    While textSize.Width > panel.Width - 40 AndAlso adjustedFont.Size > 8
                        adjustedFont = New Font(font.FontFamily, adjustedFont.Size - 0.5F, FontStyle.Bold)
                        textSize = g.MeasureString(packageName, adjustedFont)
                    End While

                    Dim x As Single = (panel.Width - textSize.Width) / 2
                    Dim y As Single = (panel.Height - textSize.Height) / 2
                    g.DrawString(packageName, adjustedFont, textBrush, x, y)
                End Using
            End Using

            ' Draw camera icon
            Using iconFont As New Font("Segoe UI Emoji", 24, FontStyle.Bold)
                Using iconBrush As New SolidBrush(Color.White)
                    g.DrawString("📷", iconFont, iconBrush, 20, panel.Height - 50)
                End Using
            End Using
        End Using
    End Sub


    ' Event handlers for card buttons
    Private Sub EditPackage_Click(sender As Object, e As EventArgs)
        Dim btn As Button = DirectCast(sender, Button)
        Dim package As TravelPackage = DirectCast(btn.Tag, TravelPackage)

        ' Switch to package management view
        currentView = "packages"
        ShowPackageManagement()
        HighlightActiveButton(packageManagementButton)

        ' Find and select the package in the grid
        For Each row As DataGridViewRow In packagesDataGrid.Rows
            If Convert.ToInt32(row.Cells("PackageID").Value) = package.PackageID Then
                packagesDataGrid.ClearSelection()
                row.Selected = True
                LoadPackageDetails()
                Exit For
            End If
        Next
    End Sub
    Private Sub DeletePackageFromCard_Click(sender As Object, e As EventArgs)
        Dim btn As Button = DirectCast(sender, Button)
        Dim package As TravelPackage = DirectCast(btn.Tag, TravelPackage)

        ' Confirm deletion
        Dim result = MessageBox.Show($"Are you sure you want to delete package '{package.Name}'? This action cannot be undone.",
                               "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
        If result = DialogResult.Yes Then
            DeletePackageById(package.PackageID)

        End If
    End Sub

    ' Method to delete package by ID
    Private Sub DeletePackageById(packageId As Integer)
        Try
            Using transaction = connection.BeginTransaction()
                Try
                    ' Delete package addons
                    Dim deleteAddonsQuery = "DELETE FROM PackageAddons WHERE PackageID = @PackageID"
                    Using cmd As New MySqlCommand(deleteAddonsQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@PackageID", packageId)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Delete package locations
                    Dim deleteLocationsQuery = "DELETE FROM PackageLocations WHERE PackageID = @PackageID"
                    Using cmd As New MySqlCommand(deleteLocationsQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@PackageID", packageId)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Delete package details
                    Dim deleteDetailsQuery = "DELETE FROM PackageDetails WHERE PackageID = @PackageID"
                    Using cmd As New MySqlCommand(deleteDetailsQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@PackageID", packageId)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Delete tour package
                    Dim deletePackageQuery = "DELETE FROM TourPackages WHERE PackageID = @PackageID"
                    Using cmd As New MySqlCommand(deletePackageQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@PackageID", packageId)
                        cmd.ExecuteNonQuery()
                    End Using

                    transaction.Commit()
                    MessageBox.Show("Package deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

                Catch ex As Exception
                    transaction.Rollback()
                    Throw ex
                End Try
            End Using

        Catch ex As Exception
            MessageBox.Show($"Error deleting package: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub




    ' Method to show package cards view
    ' Method to show package cards view


    ' Method to load and display package cards

    Private Sub LoadPackagesData(Optional whereClause As String = "")
        Try
            Dim query As String = "
        SELECT 
            pa.PackageID,
            pa.Title,
            pl.Location,
            tp.PackageType,
            tp.Price,  -- Get Price from TourPackages table
            pa.MaxSlots,
            pa.BookedSlots,
            pa.AvailableSlots,
            pa.Status,
            pa.StartDate,
            pa.EndDate
        FROM PackageAvailability pa
        JOIN TourPackages tp ON pa.PackageID = tp.PackageID  -- Join to get Price
        LEFT JOIN PackageLocations pl ON pa.PackageID = pl.PackageID"

            If Not String.IsNullOrEmpty(whereClause) Then
                query += " WHERE " + whereClause
            End If

            query += " ORDER BY pa.StartDate DESC"

            Dim adapter As New MySql.Data.MySqlClient.MySqlDataAdapter(query, connection)
            Dim dataTable As New DataTable()
            adapter.Fill(dataTable)
            packagesDataGrid.DataSource = dataTable

            ' Format columns
            FormatPackageGridColumns()
        Catch ex As Exception
            MessageBox.Show($"Error loading packages: {ex.Message}", "Data Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub FormatPackageGridColumns()
        If packagesDataGrid.Columns.Count = 0 Then Return

        packagesDataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None

        Dim visibleColumns As New Dictionary(Of String, (HeaderText As String, Width As Integer)) From {
                {"PackageID", ("ID", 50)},
                {"Title", ("Package Title", 150)},
                {"Location", ("Location", 120)},
                {"PackageType", ("Type", 80)},
                {"Price", ("Price", 80)},
                {"AvailableSlots", ("Slots", 60)},
                {"Status", ("Status", 70)},
                {"StartDate", ("Start Date", 90)},
                {"EndDate", ("End Date", 90)}
            }

        For Each col As DataGridViewColumn In packagesDataGrid.Columns
            If visibleColumns.ContainsKey(col.Name) Then
                col.HeaderText = visibleColumns(col.Name).HeaderText
                col.Width = visibleColumns(col.Name).Width
                col.Visible = True
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
                col.Resizable = DataGridViewTriState.False
            Else
                col.Visible = False
            End If
        Next

        If packagesDataGrid.Columns.Contains("StartDate") Then
            packagesDataGrid.Columns("StartDate").DefaultCellStyle.Format = "MM/dd/yyyy"
        End If
        If packagesDataGrid.Columns.Contains("EndDate") Then
            packagesDataGrid.Columns("EndDate").DefaultCellStyle.Format = "MM/dd/yyyy"
        End If
    End Sub

    Private Sub PackagesDataGrid_SelectionChanged(sender As Object, e As EventArgs)
        If packagesDataGrid.SelectedRows.Count > 0 Then
            LoadPackageDetails()
        End If
    End Sub

    Private Sub LoadPackageDetails()
        Try
            If packagesDataGrid.SelectedRows.Count = 0 Then Return

            Dim packageId = Convert.ToInt32(packagesDataGrid.SelectedRows(0).Cells("PackageID").Value)

            ' Get package basic info
            Dim packageQuery As String = "
        SELECT 
            tp.*,
            pl.Location,
            pd.FlagSymbol,
            pd.IdealFor,
            pd.Inclusions,
            pd.Highlights
        FROM TourPackages tp
        LEFT JOIN PackageLocations pl ON tp.PackageID = pl.PackageID
        LEFT JOIN PackageDetails pd ON tp.PackageID = pd.PackageID
        WHERE tp.PackageID = @PackageID"

            ' Get addons in separate query
            Dim addonsQuery As String = "
        SELECT GROUP_CONCAT(AddOnName SEPARATOR ', ') AS Addons
        FROM PackageAddOns
        WHERE PackageID = @PackageID"

            Using cmd As New MySqlCommand(packageQuery, connection)
                cmd.Parameters.AddWithValue("@PackageID", packageId)
                Using reader = cmd.ExecuteReader()
                    If reader.Read() Then
                        packageIdLabel.Text = $"Package ID: {reader("PackageID")}"
                        packageTitleTextBox.Text = reader("Title").ToString()
                        packageLocationTextBox.Text = If(reader("Location") IsNot DBNull.Value, reader("Location").ToString(), "")

                        packageTypeComboBox.SelectedItem = reader("PackageType").ToString()
                        packagePriceTextBox.Text = reader("Price").ToString()

                        ' Safe numeric value assignment
                        Dim maxSlots = Convert.ToDecimal(reader("MaxSlots"))
                        packageSlotsNumeric.Value = If(maxSlots >= packageSlotsNumeric.Minimum AndAlso maxSlots <= packageSlotsNumeric.Maximum,
                                              maxSlots, packageSlotsNumeric.Minimum)

                        packageStatusComboBox.SelectedItem = reader("Status").ToString()
                        packageDescriptionTextBox.Text = If(reader("Description") IsNot DBNull.Value, reader("Description").ToString(), "")

                        packageStartDatePicker.Value = Convert.ToDateTime(reader("StartDate"))
                        packageEndDatePicker.Value = Convert.ToDateTime(reader("EndDate"))

                        ' Safe duration assignment
                        Dim duration = Convert.ToDecimal(reader("Duration"))
                        durationNumeric.Value = If(duration >= durationNumeric.Minimum AndAlso duration <= durationNumeric.Maximum,
                                         duration, durationNumeric.Minimum)
                        durationLabel.Text = $"{durationNumeric.Value} days"
                    End If
                End Using
            End Using

            ' Load addons in separate connection/command after the first reader is closed
            Using addonCmd As New MySqlCommand(addonsQuery, connection)
                addonCmd.Parameters.AddWithValue("@PackageID", packageId)
                Dim addons = addonCmd.ExecuteScalar()
                packageAddonsTextBox.Text = If(addons IsNot DBNull.Value, addons.ToString(), "")
            End Using

        Catch ex As Exception
            MessageBox.Show($"Error loading package details: {ex.Message}", "Detail Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub CreatePackageButton_Click(sender As Object, e As EventArgs)
        CreateNewPackage()
    End Sub

    Private Sub UpdatePackageButton_Click(sender As Object, e As EventArgs) Handles updatePackageButton.Click
        Try
            If packagesDataGrid.SelectedRows.Count = 0 Then
                MessageBox.Show("Please select a package to update.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim packageId = Convert.ToInt32(packagesDataGrid.SelectedRows(0).Cells("PackageID").Value)

            ' Validate input
            If String.IsNullOrWhiteSpace(packageTitleTextBox.Text) OrElse
           String.IsNullOrWhiteSpace(packageLocationTextBox.Text) OrElse
           packageTypeComboBox.SelectedItem Is Nothing OrElse
           String.IsNullOrWhiteSpace(packagePriceTextBox.Text) OrElse
           packageStatusComboBox.SelectedItem Is Nothing Then
                MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim price As Decimal
            If Not Decimal.TryParse(packagePriceTextBox.Text, price) Then
                MessageBox.Show("Please enter a valid price.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Using transaction = connection.BeginTransaction()
                Try
                    ' Update tour package
                    Dim updatePackageQuery = "
                    UPDATE TourPackages 
                    SET Title = @Title,
                        PackageType = @PackageType,
                        Price = @Price,
                        Description = @Description,
                        MaxSlots = @MaxSlots,
                        Status = @Status,
                        StartDate = @StartDate,
                        EndDate = @EndDate,
                        UpdatedAt = NOW()
                    WHERE PackageID = @PackageID"

                    Using cmd As New MySqlCommand(updatePackageQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@Title", packageTitleTextBox.Text)
                        cmd.Parameters.AddWithValue("@PackageType", packageTypeComboBox.SelectedItem.ToString())
                        cmd.Parameters.AddWithValue("@Price", price)
                        cmd.Parameters.AddWithValue("@Description", packageDescriptionTextBox.Text)
                        cmd.Parameters.AddWithValue("@MaxSlots", packageSlotsNumeric.Value)
                        cmd.Parameters.AddWithValue("@Status", packageStatusComboBox.SelectedItem.ToString())
                        cmd.Parameters.AddWithValue("@StartDate", packageStartDatePicker.Value.Date)
                        cmd.Parameters.AddWithValue("@EndDate", packageEndDatePicker.Value.Date)
                        cmd.Parameters.AddWithValue("@PackageID", packageId)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Update package location
                    Dim updateLocationQuery = "
                    UPDATE PackageLocations 
                    SET Location = @Location
                    WHERE PackageID = @PackageID"

                    Using cmd As New MySqlCommand(updateLocationQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@Location", packageLocationTextBox.Text)
                        cmd.Parameters.AddWithValue("@PackageID", packageId)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Update package addons
                    Dim deleteAddonsQuery = "DELETE FROM PackageAddOns WHERE PackageID = @PackageID"
                    Using cmd As New MySqlCommand(deleteAddonsQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@PackageID", packageId)
                        cmd.ExecuteNonQuery()
                    End Using

                    If Not String.IsNullOrWhiteSpace(packageAddonsTextBox.Text) Then
                        Dim addons = packageAddonsTextBox.Text.Split(","c)
                        For Each addon In addons
                            If Not String.IsNullOrWhiteSpace(addon.Trim()) Then
                                Dim insertAddonQuery = "
                                INSERT INTO PackageAddOns 
                                (PackageID, AddOnName, Price, Unit, IsActive)
                                VALUES (@PackageID, @AddOnName, 0, 'per person', 1)"

                                Using cmd As New MySqlCommand(insertAddonQuery, connection, transaction)
                                    cmd.Parameters.AddWithValue("@PackageID", packageId)
                                    cmd.Parameters.AddWithValue("@AddOnName", addon.Trim())
                                    cmd.ExecuteNonQuery()
                                End Using
                            End If
                        Next
                    End If

                    transaction.Commit()
                    MessageBox.Show("Package updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

                    ' Refresh data
                    LoadPackagesData()
                    LoadPackageDetails()

                Catch ex As Exception
                    transaction.Rollback()
                    MessageBox.Show($"Error updating package: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End Using

        Catch ex As Exception
            MessageBox.Show($"Error updating package: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub DeletePackageButton_Click(sender As Object, e As EventArgs)
        DeletePackageRecord()
    End Sub

    Private Sub ResetPackageButton_Click(sender As Object, e As EventArgs)
        ClearPackageDetails()
    End Sub

    Private Sub CreateNewPackage()
        Try
            ' Validate input
            If String.IsNullOrWhiteSpace(packageTitleTextBox.Text) OrElse
           String.IsNullOrWhiteSpace(packageLocationTextBox.Text) OrElse
           packageTypeComboBox.SelectedItem Is Nothing OrElse
           String.IsNullOrWhiteSpace(packagePriceTextBox.Text) OrElse
           packageStatusComboBox.SelectedItem Is Nothing Then
                MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Validate dates
            If packageEndDatePicker.Value.Date <= packageStartDatePicker.Value.Date Then
                MessageBox.Show("End date must be after start date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim price As Decimal
            If Not Decimal.TryParse(packagePriceTextBox.Text, price) Then
                MessageBox.Show("Please enter a valid price.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Calculate duration
            Dim duration As Integer = CInt(durationNumeric.Value)

            Using transaction = connection.BeginTransaction()
                Try
                    ' Insert tour package with duration
                    Dim insertPackageQuery = "
                                                INSERT INTO TourPackages 
                                                (Title, PackageType, Price, Description, AvailableSlots, Status, 
                                                 StartDate, EndDate, Duration, CreatedAt, UpdatedAt)
                                                VALUES (@Title, @PackageType, @Price, @Description, @AvailableSlots, @Status,
                                                        @StartDate, @EndDate, @Duration, NOW(), NOW())"

                    Dim packageId As Integer
                    Using cmd As New MySqlCommand(insertPackageQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@Title", packageTitleTextBox.Text)
                        cmd.Parameters.AddWithValue("@PackageType", packageTypeComboBox.SelectedItem.ToString())
                        cmd.Parameters.AddWithValue("@Price", price)
                        cmd.Parameters.AddWithValue("@Description", packageDescriptionTextBox.Text)
                        cmd.Parameters.AddWithValue("@AvailableSlots", packageSlotsNumeric.Value)
                        cmd.Parameters.AddWithValue("@Status", packageStatusComboBox.SelectedItem.ToString())
                        cmd.Parameters.AddWithValue("@StartDate", packageStartDatePicker.Value.Date)
                        cmd.Parameters.AddWithValue("@EndDate", packageEndDatePicker.Value.Date)
                        cmd.Parameters.AddWithValue("@Duration", duration)
                        cmd.ExecuteNonQuery()
                        packageId = cmd.LastInsertedId
                    End Using

                    ' Insert package location
                    Dim insertLocationQuery = "INSERT INTO PackageLocations (PackageID, Location) VALUES (@PackageID, @Location)"
                    Using cmd As New MySqlCommand(insertLocationQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@PackageID", packageId)
                        cmd.Parameters.AddWithValue("@Location", packageLocationTextBox.Text)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Insert basic package details
                    Dim insertDetailsQuery = "INSERT INTO PackageDetails (PackageID, PackageType, FlagSymbol, IdealFor) VALUES (@PackageID, @PackageType, @FlagSymbol, @IdealFor)"
                    Using cmd As New MySqlCommand(insertDetailsQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@PackageID", packageId)
                        cmd.Parameters.AddWithValue("@PackageType", packageTypeComboBox.SelectedItem.ToString())
                        cmd.Parameters.AddWithValue("@FlagSymbol", "🏝️") ' Default flag
                        cmd.Parameters.AddWithValue("@IdealFor", "Everyone") ' Default ideal for
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Insert addons if provided
                    If Not String.IsNullOrWhiteSpace(packageAddonsTextBox.Text) Then
                        Dim addons = packageAddonsTextBox.Text.Split(","c)
                        For Each addon In addons
                            If Not String.IsNullOrWhiteSpace(addon.Trim()) Then
                                Dim insertAddonQuery = "INSERT INTO PackageAddons (PackageID, AddonName) VALUES (@PackageID, @AddonName)"
                                Using cmd As New MySqlCommand(insertAddonQuery, connection, transaction)
                                    cmd.Parameters.AddWithValue("@PackageID", packageId)
                                    cmd.Parameters.AddWithValue("@AddonName", addon.Trim())
                                    cmd.ExecuteNonQuery()
                                End Using
                            End If
                        Next
                    End If

                    transaction.Commit()

                    If PictureBoxImage.Image IsNot Nothing AndAlso Not String.IsNullOrEmpty(selectedImagePath) Then
                        If Not SavePackageImage(packageId) Then
                            ' If image save fails, rollback transaction
                            transaction.Rollback()
                            MessageBox.Show("Failed to save package image. Package creation cancelled.", "Error")
                            Return
                        End If
                    End If

                    MessageBox.Show("Package created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    LoadPackagesData()
                    ClearPackageDetails()

                    ' Auto-generate and display the new package card
                    GenerateAndShowNewPackageCard(packageId)

                Catch ex As Exception
                    transaction.Rollback()
                    Throw ex
                End Try
            End Using

        Catch ex As Exception
            MessageBox.Show($"Error creating package: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub CalculateDuration(sender As Object, e As EventArgs)
        ' Only calculate duration during package creation
        If packageIdLabel.Text = "Package ID: --" Then
            Dim duration As TimeSpan = packageEndDatePicker.Value.Date - packageStartDatePicker.Value.Date
            durationNumeric.Value = duration.Days
            durationLabel.Text = $"{duration.Days} days"
        End If
    End Sub

    Private Sub UpdatePackageRecord()
        Try
            If packagesDataGrid.SelectedRows.Count = 0 Then
                MessageBox.Show("Please select a package to update.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim packageId = Convert.ToInt32(packagesDataGrid.SelectedRows(0).Cells("PackageID").Value)

            ' Get current package data from database
            Dim currentPackage As New Dictionary(Of String, Object)
            Dim getCurrentQuery = "SELECT * FROM TourPackages WHERE PackageID = @PackageID"
            Using cmd As New MySqlCommand(getCurrentQuery, connection)
                cmd.Parameters.AddWithValue("@PackageID", packageId)
                Using reader = cmd.ExecuteReader()
                    If reader.Read() Then
                        For i As Integer = 0 To reader.FieldCount - 1
                            currentPackage.Add(reader.GetName(i), reader(i))
                        Next
                    End If
                End Using
            End Using

            ' Prepare update statement with only changed fields
            Dim updateFields As New List(Of String)
            Dim parameters As New Dictionary(Of String, Object)
            parameters.Add("@PackageID", packageId)

            ' Check each field for changes
            If Not String.IsNullOrWhiteSpace(packageTitleTextBox.Text) AndAlso
               packageTitleTextBox.Text <> currentPackage("Title").ToString() Then
                updateFields.Add("Title = @Title")
                parameters.Add("@Title", packageTitleTextBox.Text)
            End If

            If packageTypeComboBox.SelectedItem IsNot Nothing AndAlso
               packageTypeComboBox.SelectedItem.ToString() <> currentPackage("PackageType").ToString() Then
                updateFields.Add("PackageType = @PackageType")
                parameters.Add("@PackageType", packageTypeComboBox.SelectedItem.ToString())
            End If

            If Not String.IsNullOrWhiteSpace(packagePriceTextBox.Text) Then
                Dim newPrice As Decimal
                If Decimal.TryParse(packagePriceTextBox.Text, newPrice) AndAlso
                   newPrice <> Convert.ToDecimal(currentPackage("Price")) Then
                    updateFields.Add("Price = @Price")
                    parameters.Add("@Price", newPrice)
                End If
            End If

            If packageSlotsNumeric.Value <> Convert.ToDecimal(currentPackage("AvailableSlots")) Then
                updateFields.Add("AvailableSlots = @AvailableSlots")
                parameters.Add("@AvailableSlots", packageSlotsNumeric.Value)
            End If

            If packageStatusComboBox.SelectedItem IsNot Nothing AndAlso
               packageStatusComboBox.SelectedItem.ToString() <> currentPackage("Status").ToString() Then
                updateFields.Add("Status = @Status")
                parameters.Add("@Status", packageStatusComboBox.SelectedItem.ToString())
            End If

            If packageStartDatePicker.Value.Date <> Convert.ToDateTime(currentPackage("StartDate")).Date Then
                updateFields.Add("StartDate = @StartDate")
                parameters.Add("@StartDate", packageStartDatePicker.Value.Date)
            End If

            If packageEndDatePicker.Value.Date <> Convert.ToDateTime(currentPackage("EndDate")).Date Then
                updateFields.Add("EndDate = @EndDate")
                parameters.Add("@EndDate", packageEndDatePicker.Value.Date)
            End If

            ' If no fields were changed, show message and return
            If updateFields.Count = 0 Then
                MessageBox.Show("No changes detected to update.", "No Changes", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            ' Build and execute the update query
            Dim updatePackageQuery = $"UPDATE TourPackages SET {String.Join(", ", updateFields)}, UpdatedAt = NOW() WHERE PackageID = @PackageID"

            Using transaction = connection.BeginTransaction()
                Try
                    ' Update tour package
                    Using cmd As New MySqlCommand(updatePackageQuery, connection, transaction)
                        For Each param In parameters
                            cmd.Parameters.AddWithValue(param.Key, param.Value)
                        Next
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Update package location if changed
                    If Not String.IsNullOrWhiteSpace(packageLocationTextBox.Text) Then
                        ' Check if location exists first
                        Dim locationExistsQuery = "SELECT COUNT(*) FROM PackageLocations WHERE PackageID = @PackageID"
                        Dim locationExists As Boolean = False

                        Using cmd As New MySqlCommand(locationExistsQuery, connection, transaction)
                            cmd.Parameters.AddWithValue("@PackageID", packageId)
                            locationExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0
                        End Using

                        If locationExists Then
                            ' Update existing location
                            Dim updateLocationQuery = "UPDATE PackageLocations SET Location = @Location WHERE PackageID = @PackageID"
                            Using cmd As New MySqlCommand(updateLocationQuery, connection, transaction)
                                cmd.Parameters.AddWithValue("@Location", packageLocationTextBox.Text)
                                cmd.Parameters.AddWithValue("@PackageID", packageId)
                                cmd.ExecuteNonQuery()
                            End Using
                        Else
                            ' Insert new location
                            Dim insertLocationQuery = "INSERT INTO PackageLocations (PackageID, Location) VALUES (@PackageID, @Location)"
                            Using cmd As New MySqlCommand(insertLocationQuery, connection, transaction)
                                cmd.Parameters.AddWithValue("@PackageID", packageId)
                                cmd.Parameters.AddWithValue("@Location", packageLocationTextBox.Text)
                                cmd.ExecuteNonQuery()
                            End Using
                        End If
                    End If

                    ' Update addons if changed
                    If Not String.IsNullOrWhiteSpace(packageAddonsTextBox.Text) Then
                        ' Delete existing addons
                        Dim deleteAddonsQuery = "DELETE FROM PackageAddons WHERE PackageID = @PackageID"
                        Using cmd As New MySqlCommand(deleteAddonsQuery, connection, transaction)
                            cmd.Parameters.AddWithValue("@PackageID", packageId)
                            cmd.ExecuteNonQuery()
                        End Using

                        ' Insert new addons
                        Dim addons = packageAddonsTextBox.Text.Split(","c)
                        For Each addon In addons
                            If Not String.IsNullOrWhiteSpace(addon.Trim()) Then
                                Dim insertAddonQuery = "INSERT INTO PackageAddons (PackageID, AddonName) VALUES (@PackageID, @AddonName)"
                                Using cmd As New MySqlCommand(insertAddonQuery, connection, transaction)
                                    cmd.Parameters.AddWithValue("@PackageID", packageId)
                                    cmd.Parameters.AddWithValue("@AddonName", addon.Trim())
                                    cmd.ExecuteNonQuery()
                                End Using
                            End If
                        Next
                    End If

                    transaction.Commit()
                    MessageBox.Show("Package updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    LoadPackagesData()
                    LoadPackageDetails()

                Catch ex As Exception
                    transaction.Rollback()
                    Throw ex
                End Try
            End Using

        Catch ex As Exception
            MessageBox.Show($"Error updating package: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub DeletePackageRecord()
        Try
            If packagesDataGrid.SelectedRows.Count = 0 Then
                MessageBox.Show("Please select a package to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim packageId = Convert.ToInt32(packagesDataGrid.SelectedRows(0).Cells("PackageID").Value)
            Dim packageTitle = packagesDataGrid.SelectedRows(0).Cells("Title").Value.ToString()

            ' Confirm deletion
            Dim result = MessageBox.Show($"Are you sure you want to delete package '{packageTitle}'? This action cannot be undone.",
                                           "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
            If result <> DialogResult.Yes Then Return

            ' Check if package has existing bookings
            Dim checkBookingsQuery = "SELECT COUNT(*) FROM Bookings WHERE PackageID = @PackageID AND Status NOT IN ('Cancelled')"
            Using cmd As New MySqlCommand(checkBookingsQuery, connection)
                cmd.Parameters.AddWithValue("@PackageID", packageId)
                Dim bookingCount = Convert.ToInt32(cmd.ExecuteScalar())
                If bookingCount > 0 Then
                    MessageBox.Show("Cannot delete package with existing active bookings. Please cancel all bookings first or set package status to Inactive.",
                                      "Cannot Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If
            End Using

            Using transaction = connection.BeginTransaction()
                Try
                    ' Delete package addons
                    Dim deleteAddonsQuery = "DELETE FROM PackageAddons WHERE PackageID = @PackageID"
                    Using cmd As New MySqlCommand(deleteAddonsQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@PackageID", packageId)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Delete package locations
                    Dim deleteLocationsQuery = "DELETE FROM PackageLocations WHERE PackageID = @PackageID"
                    Using cmd As New MySqlCommand(deleteLocationsQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@PackageID", packageId)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Delete tour package
                    Dim deletePackageQuery = "DELETE FROM TourPackages WHERE PackageID = @PackageID"
                    Using cmd As New MySqlCommand(deletePackageQuery, connection, transaction)
                        cmd.Parameters.AddWithValue("@PackageID", packageId)
                        cmd.ExecuteNonQuery()
                    End Using

                    transaction.Commit()
                    MessageBox.Show("Package deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    LoadPackagesData()
                    ClearPackageDetails()

                Catch ex As Exception
                    transaction.Rollback()
                    Throw ex
                End Try
            End Using

        Catch ex As Exception
            MessageBox.Show($"Error deleting package: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ClearPackageDetails()
        packageIdLabel.Text = "Package ID: --"
        packageTitleTextBox.Clear()
        packageLocationTextBox.Clear()
        packageTypeComboBox.SelectedIndex = -1
        packagePriceTextBox.Clear()
        packageSlotsNumeric.Value = 1
        packageStatusComboBox.SelectedIndex = -1
        packageDescriptionTextBox.Clear()
        packageAddonsTextBox.Clear()
        packageStartDatePicker.Value = DateTime.Now
        packageEndDatePicker.Value = DateTime.Now.AddDays(1)
        durationNumeric.Value = 1
        durationLabel.Text = "1 day"
        durationNumeric.Enabled = True ' Enable duration editing for new packages
        PictureBoxImage.Image = Nothing
    End Sub

    Private Sub UpdateCompletedTrips()
        Using transaction = connection.BeginTransaction()
            Try
                connection = New MySqlConnection(connectionString)
                connection.Open()

                ' Get bookings that need to be completed
                Dim getBookingsQuery As String = "SELECT BookingID, BookingReference, Status FROM Bookings " &
                                       "WHERE UserID = @UserID " &
                                       "AND Status IN ('Confirmed', 'Pending') " &
                                       "AND DATE(EndDate) < CURDATE()"

                Dim bookingsToComplete As New List(Of Tuple(Of Integer, String, String))
                Using getBookingsCmd As New MySql.Data.MySqlClient.MySqlCommand(getBookingsQuery, connection, transaction)
                    getBookingsCmd.Parameters.AddWithValue("@UserID", UserID)
                    Using reader As MySql.Data.MySqlClient.MySqlDataReader = getBookingsCmd.ExecuteReader()
                        While reader.Read()
                            bookingsToComplete.Add(New Tuple(Of Integer, String, String)(
                       Convert.ToInt32(reader("BookingID")),
                       reader("BookingReference").ToString(),
                       reader("Status").ToString()
                   ))
                        End While
                    End Using
                End Using

                If bookingsToComplete.Count = 0 Then
                    Console.WriteLine("No trips found that need to be marked as completed.")
                    Return
                End If

                ' Update trips that are past their end date to completed
                Dim updateQuery As String = "UPDATE Bookings SET Status = 'Completed', UpdatedAt = NOW() " &
                      "WHERE UserID = @UserID " &
                      "AND Status IN ('Confirmed', 'Pending') " &
                      "AND DATE(EndDate) < CURDATE()"

                Dim updatedRows As Integer = 0
                Using updateCmd As New MySql.Data.MySqlClient.MySqlCommand(updateQuery, connection, transaction)
                    updateCmd.Parameters.AddWithValue("@UserID", UserID)
                    updatedRows = updateCmd.ExecuteNonQuery()
                End Using

                ' Update payment status for completed bookings
                Dim updatePaymentQuery As String = "UPDATE BookingPayments bp " &
                             "INNER JOIN Bookings b ON bp.BookingID = b.BookingID " &
                             "SET bp.PaymentStatus = 'Completed', bp.UpdatedAt = NOW() " &
                             "WHERE b.UserID = @UserID AND b.Status = 'Completed'"

                Using updatePaymentCmd As New MySql.Data.MySqlClient.MySqlCommand(updatePaymentQuery, connection, transaction)
                    updatePaymentCmd.Parameters.AddWithValue("@UserID", UserID)
                    updatePaymentCmd.ExecuteNonQuery()
                End Using

                ' Add entries to BookingHistory for audit trail for each completed booking
                For Each booking In bookingsToComplete
                    Dim insertHistoryQuery As String = "INSERT INTO BookingHistory " &
                                 "(BookingID, UserID, ActionType, ActionDescription, PreviousStatus, NewStatus, ModifiedBy) " &
                                 "VALUES (@BookingID, @UserID, 'Completed', 'Trip automatically marked as completed', @PreviousStatus, 'Completed', @UserID)"

                    Using insertHistoryCmd As New MySql.Data.MySqlClient.MySqlCommand(insertHistoryQuery, connection, transaction)
                        insertHistoryCmd.Parameters.AddWithValue("@BookingID", booking.Item1)
                        insertHistoryCmd.Parameters.AddWithValue("@UserID", UserID)
                        insertHistoryCmd.Parameters.AddWithValue("@PreviousStatus", booking.Item3)
                        insertHistoryCmd.ExecuteNonQuery()
                    End Using
                Next

                transaction.Commit()

                Console.WriteLine($"Successfully processed {updatedRows} completed trips at {DateTime.Now}")
            Catch ex As Exception
                Try
                    If transaction IsNot Nothing Then
                        transaction.Rollback()
                        Console.WriteLine($"Transaction rolled back for completed trips update: {ex.Message}")
                    End If
                Catch rollbackEx As Exception
                    Console.WriteLine($"Error during rollback: {rollbackEx.Message}")
                End Try

                MessageBox.Show($"Error updating trip statuses: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

            Finally
                Try
                    If transaction IsNot Nothing Then
                        transaction.Dispose()
                    End If
                Catch disposeEx As Exception
                    Console.WriteLine($"Error disposing transaction: {disposeEx.Message}")
                End Try

                Try
                    If connection IsNot Nothing AndAlso connection.State = ConnectionState.Open Then
                        connection.Close()
                    End If
                Catch closeEx As Exception
                    Console.WriteLine($"Error closing connection: {closeEx.Message}")
                End Try

            End Try
        End Using
    End Sub

    Private Function GetUserIdFromBooking(bookingId As Integer, transaction As MySql.Data.MySqlClient.MySqlTransaction) As Integer
        Try
            Dim query = "SELECT UserID FROM Bookings WHERE BookingID = @BookingID"
            Using cmd As New MySqlCommand(query, connection, transaction)
                cmd.Parameters.AddWithValue("@BookingID", bookingId)
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                    Return Convert.ToInt32(result)
                End If
            End Using
        Catch ex As Exception
            ' Log error but don't throw to avoid breaking the main update
            Console.WriteLine($"Error getting UserID: {ex.Message}")
        End Try
        Return 0
    End Function

    Private Sub HighlightActiveButton(activeButton As Button)
        ' Reset all buttons to default color including the view package cards button
        Dim buttons As New List(Of Button) From {
            allBookingsButton, pendingBookingsButton, confirmedBookingsButton,
            completedBookingsButton, cancelledBookingsButton, financialManagementButton,
            packageManagementButton, createNewPackageButton
        }

        ' Find the view cards button if it exists and add it to the list
        For Each ctrl As Control In sidePanel.Controls
            If TypeOf ctrl Is Button AndAlso ctrl.Text.Contains("View Package Cards") Then
                buttons.Add(DirectCast(ctrl, Button))
                Exit For
            End If
        Next

        ' Reset all buttons to default color
        For Each btn As Button In buttons
            If btn IsNot Nothing Then
                btn.BackColor = Color.FromArgb(71, 85, 105)
            End If
        Next

        ' Highlight only the active button
        If activeButton IsNot Nothing Then
            activeButton.BackColor = Color.FromArgb(37, 99, 235)
        End If
    End Sub

    Private Sub ClearBookingDetails()
        bookingIdLabel.Text = "Booking ID: --"
        bookingReferenceLabel.Text = "Reference: --"
        customerNameTextBox.Clear()
        customerEmailTextBox.Clear()
        customerPhoneTextBox.Clear()
        packageTitleLabel.Text = "Package: --"
        packageLocationLabel.Text = "Location: --"
        packageTypeLabel.Text = "Type: --"
        travelDatePicker.Value = DateTime.Now
        endDatePicker.Value = DateTime.Now
        numberOfPeopleNumeric.Value = 1
        finalAmountTextBox.Clear()
        statusComboBox.SelectedIndex = -1
        paymentStatusComboBox.SelectedIndex = -1
        paymentMethodLabel.Text = "Payment Method: --"
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        ' Close database connection when form is closing
        If connection IsNot Nothing AndAlso connection.State = ConnectionState.Open Then
            connection.Close()
        End If
        MyBase.OnFormClosing(e)
    End Sub

    Private Sub SearchTextBox_KeyPress(sender As Object, e As KeyPressEventArgs) Handles searchTextBox.KeyPress
        If e.KeyChar = Convert.ToChar(Keys.Enter) Then
            PerformBookingSearch()
            e.Handled = True
        End If
    End Sub

    ' Method to export bookings to CSV
    Private Sub ExportToCSV()
        Try
            Dim saveDialog As New SaveFileDialog()
            saveDialog.Filter = "CSV files (*.csv)|*.csv"
            saveDialog.Title = "Export Bookings to CSV"
            saveDialog.FileName = $"LakbayPH_Bookings_{DateTime.Now:yyyyMMdd}"

            If saveDialog.ShowDialog() = DialogResult.OK Then
                Using writer As New System.IO.StreamWriter(saveDialog.FileName)
                    ' Write headers
                    Dim headers As New List(Of String)
                    For Each column As DataGridViewColumn In bookingsDataGrid.Columns
                        If column.Visible Then
                            headers.Add(column.HeaderText)
                        End If
                    Next
                    writer.WriteLine(String.Join(",", headers))

                    ' Write data rows
                    For Each row As DataGridViewRow In bookingsDataGrid.Rows
                        If Not row.IsNewRow Then
                            Dim values As New List(Of String)
                            For Each column As DataGridViewColumn In bookingsDataGrid.Columns
                                If column.Visible Then
                                    Dim cellValue = If(row.Cells(column.Index).Value?.ToString(), "")
                                    ' Escape commas and quotes in CSV
                                    If cellValue.Contains(",") OrElse cellValue.Contains("""") Then
                                        cellValue = """" & cellValue.Replace("""", """""") & """"
                                    End If
                                    values.Add(cellValue)
                                End If
                            Next
                            writer.WriteLine(String.Join(",", values))
                        End If
                    Next
                End Using

                MessageBox.Show($"Bookings exported successfully to {saveDialog.FileName}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

        Catch ex As Exception
            MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class