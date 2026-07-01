Imports MySql.Data.MySqlClient
Imports projectv2.BookingItems
Imports System.Text.RegularExpressions

Public Class FinancialHelpers
    Public Shared Function CleanCurrencyString(currencyString As String) As Decimal
        ' Remove all non-numeric characters except decimal point
        If String.IsNullOrWhiteSpace(currencyString) Then
            Return 0D
        End If

        Dim cleaned = Regex.Replace(currencyString, "[^\d.]", "")

        Dim result As Decimal
        If Decimal.TryParse(cleaned, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, result) Then
            Return result
        Else
            Throw New FormatException($"Could not parse currency string: {currencyString}")
        End If
    End Function

    Public Shared Function FormatAsCurrency(amount As Decimal) As String
        Return "₱" & amount.ToString("N2")
    End Function
End Class
Public Class FinancialManagementForms
    Inherits Form


    ' Database connection
    Dim connectionString As String = "Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;"

    Private isFormLoaded As Boolean = False

    ' Form controls
    Private WithEvents tabControl As TabControl
    Private WithEvents incomeTab As TabPage
    Private WithEvents expenseTab As TabPage
    Private WithEvents reportsTab As TabPage
    Private WithEvents dashboardTab As TabPage
    Private WithEvents btnRefresh As Button
    Private WithEvents btnRefreshBookings As Button

    Private totalGrossIncome As Decimal = 0
    Private totalCommissionIncome As Decimal = 0
    Private totalExpenses As Decimal = 0

    ' Dashboard controls
    Private lblTotalIncome As Label
    Private lblTotalExpenses As Label
    Private lblNetProfit As Label
    Private lblProfitMargin As Label
    Private chartPanel As Panel
    Private WithEvents dgvRecent As DataGridView
    Private WithEvents cmbTransactionType As ComboBox


    ' Income controls
    Private WithEvents dgvIncome As DataGridView
    Private WithEvents btnAddIncome As Button
    Private WithEvents btnEditIncome As Button
    Private WithEvents btnDeleteIncome As Button
    Private WithEvents cmbIncomeSource As ComboBox
    Private WithEvents dtpIncomeDate As DateTimePicker
    Private WithEvents txtIncomeDescription As TextBox
    Private WithEvents lstBookingIds As CheckedListBox
    Private lblComputedAmount As Label
    Private lblComputedCommission As Label
    Private lblComputedNet As Label
    Private lblCommissionIncome As Label
    ' Expense controls
    Private WithEvents dgvExpenses As DataGridView
    Private WithEvents btnAddExpense As Button
    Private WithEvents btnEditExpense As Button
    Private WithEvents btnDeleteExpense As Button
    Private WithEvents cmbExpenseCategory As ComboBox
    Private WithEvents dtpExpenseDate As DateTimePicker
    Private WithEvents txtExpenseDescription As TextBox
    Private WithEvents txtReceiptNumber As TextBox
    Private WithEvents cmbPaymentMethod As ComboBox
    Private WithEvents lstExpenseBookingIds As CheckedListBox
    Private lblComputedExpenseAmount As Label

    ' Report controls

    Private WithEvents dtpReportStart As DateTimePicker
    Private WithEvents dtpReportEnd As DateTimePicker
    Private WithEvents btnGenerateReport As Button
    Private WithEvents dgvReports As DataGridView
    Private WithEvents btnExportReport As Button
    Private dgvReport As DataGridView
    Private lblReportIncome As Label
    Private lblReportExpenses As Label
    Private lblReportProfit As Label
    Private lblReportMargin As Label
    Private WithEvents txtExpenseAmount As TextBox
    Private dgvBookings As DataGridView
    Public Property bottomPanel As Object


    Public Sub New()
        ' This call is required by the designer
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call
        Try
            ' Only load basic data, not dashboard data yet
            LoadIncomeSources()
            LoadExpenseCategories()
            LoadBookings()
            LoadIncomeData()
            LoadExpenseData()
            ' Don't call LoadDashboardData() here - it will be called in Form_Load
        Catch ex As Exception
            MessageBox.Show("Error initializing Financial Management: " & ex.Message & vbCrLf & vbCrLf & "Stack Trace: " & ex.StackTrace, "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.Close()
        End Try
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        If isFormLoaded Then
            ResizeDataGridView()
        End If
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "Financial Management - LakbayPH"
        Me.Size = New Size(1200, 800)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.MinimumSize = New Size(1000, 600)
        Me.WindowState = FormWindowState.Maximized
        Me.DoubleBuffered = True


        ' Initialize TabControl if not already done
        If tabControl Is Nothing Then
            tabControl = New TabControl()
        End If

        tabControl.Dock = DockStyle.Fill
        tabControl.Font = New Font("Arial", 10)

        ' Create tabs
        CreateDashboardTab()
        CreateIncomeTab()
        CreateExpenseTab()
        CreateReportsTab()

        Me.Controls.Add(tabControl)

        ' Set the form as loaded AFTER all components are created
        isFormLoaded = True
    End Sub



    Private Sub ResizeDataGridView()
        If dgvRecent IsNot Nothing AndAlso isFormLoaded Then
            Try
                ' Get the parent container size
                Dim containerPanel = dgvRecent.Parent
                If containerPanel IsNot Nothing Then
                    ' Set proper size and dock
                    dgvRecent.Dock = DockStyle.None
                    dgvRecent.Size = New Size(containerPanel.Width - 100, containerPanel.Height - 40)
                    dgvRecent.Location = New Point(50, 20)
                    dgvRecent.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
                End If
            Catch ex As Exception
                ' Handle silently
            End Try
        End If
    End Sub

    Private Sub InitializeDashboardComponents()
        Try
            ' Ensure dgvRecent is properly initialized
            If dgvRecent Is Nothing Then
                dgvRecent = New DataGridView()
                dgvRecent.Dock = DockStyle.Fill
                dgvRecent.ReadOnly = True
                dgvRecent.AllowUserToAddRows = False
                dgvRecent.AllowUserToDeleteRows = False
                dgvRecent.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            End If

            ' Initialize columns if needed
            If dgvRecent.Columns.Count = 0 Then
                SetupAllColumns()
            End If

            ' Ensure cmbTransactionType is initialized
            If cmbTransactionType Is Nothing Then
                cmbTransactionType = New ComboBox()
                cmbTransactionType.Items.AddRange({"All", "Income", "Expense"})
                cmbTransactionType.SelectedIndex = 0
                cmbTransactionType.DropDownStyle = ComboBoxStyle.DropDownList
            End If

        Catch ex As Exception
            ' Handle initialization errors silently
        End Try
    End Sub

    Private Sub CreateDashboardTab()
        dashboardTab = New TabPage("Dashboard")
        dashboardTab.BackColor = Color.White
        dashboardTab.Padding = New Padding(10)

        ' Initialize cmbTransactionType first
        cmbTransactionType = New ComboBox()
        cmbTransactionType.Items.AddRange({"All", "Income", "Expense"})
        cmbTransactionType.SelectedIndex = 0
        cmbTransactionType.DropDownStyle = ComboBoxStyle.DropDownList

        ' Create and configure the DataGridView
        dgvRecent = New DataGridView()
        dgvRecent.ReadOnly = True
        dgvRecent.AllowUserToAddRows = False
        dgvRecent.AllowUserToDeleteRows = False
        dgvRecent.ColumnHeadersHeight = 35
        dgvRecent.RowTemplate.Height = 30
        dgvRecent.ColumnHeadersDefaultCellStyle.Font = New Font("Arial", 12, FontStyle.Bold)
        dgvRecent.DefaultCellStyle.Font = New Font("Arial", 11)
        dgvRecent.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        ' Initialize the grid columns
        SetupAllColumns()

        ' Main container with 2 rows (summary and recent transactions)
        Dim mainLayout As New TableLayoutPanel()
        mainLayout.Dock = DockStyle.Fill
        mainLayout.RowCount = 2
        mainLayout.ColumnCount = 1
        mainLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 300))
        mainLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

        ' ================= SUMMARY SECTION =================
        Dim summaryPanel As New Panel()
        summaryPanel.Dock = DockStyle.Fill
        summaryPanel.Padding = New Padding(5)

        Dim metricsLayout As New TableLayoutPanel()
        metricsLayout.Dock = DockStyle.Fill
        metricsLayout.ColumnCount = 5
        metricsLayout.RowCount = 1
        metricsLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 20))
        metricsLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 20))
        metricsLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 20))
        metricsLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 20))
        metricsLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 20))

        ' 1. Total Gross Income Box
        Dim incomeBox As Panel = CreateColoredBox(
    "TOTAL GROSS INCOME",
    "₱" & totalGrossIncome.ToString("N2"),
    Color.FromArgb(40, 167, 69))
        lblTotalIncome = CType(incomeBox.Controls(0), Label)
        metricsLayout.Controls.Add(incomeBox, 0, 0)

        ' 2. Commission Income Box
        Dim commissionBox As Panel = CreateColoredBox(
    "COMMISSION INCOME",
    "₱" & totalCommissionIncome.ToString("N2"),
    Color.FromArgb(255, 193, 7))
        lblCommissionIncome = CType(commissionBox.Controls(0), Label)
        metricsLayout.Controls.Add(commissionBox, 1, 0)

        ' 3. Total Expenses Box
        Dim expenseBox As Panel = CreateColoredBox(
    "TOTAL EXPENSES",
    "₱" & totalExpenses.ToString("N2"),
    Color.FromArgb(220, 53, 69))
        lblTotalExpenses = CType(expenseBox.Controls(0), Label)
        metricsLayout.Controls.Add(expenseBox, 2, 0)

        ' 4. Net Profit Box
        Dim profitBox As Panel = CreateColoredBox(
    "NET PROFIT (20%)",
    "₱" & (totalCommissionIncome - totalExpenses).ToString("N2"),
    Color.FromArgb(0, 123, 255))
        lblNetProfit = CType(profitBox.Controls(0), Label)
        metricsLayout.Controls.Add(profitBox, 3, 0)

        ' 5. Profit Percentage Box
        Dim marginBox As Panel = CreateColoredBox(
    "PROFIT PERCENTAGE",
    If(totalCommissionIncome > 0, ((totalCommissionIncome - totalExpenses) / totalCommissionIncome) * 100, 0).ToString("N1") & "%",
    Color.FromArgb(102, 16, 242))
        lblProfitMargin = CType(marginBox.Controls(0), Label)
        metricsLayout.Controls.Add(marginBox, 4, 0)

        summaryPanel.Controls.Add(metricsLayout)
        mainLayout.Controls.Add(summaryPanel, 0, 0)

        ' ================= RECENT TRANSACTIONS SECTION =================
        Dim recentPanel As New Panel()
        recentPanel.Dock = DockStyle.Fill
        recentPanel.Padding = New Padding(20)

        Dim lblRecent As New Label()
        lblRecent.Text = "RECENT TRANSACTIONS"
        lblRecent.Font = New Font("Arial", 14, FontStyle.Bold)
        lblRecent.Dock = DockStyle.Top
        lblRecent.Height = 40
        lblRecent.TextAlign = ContentAlignment.MiddleCenter

        ' Add transaction type filter
        Dim filterPanel As New Panel()
        filterPanel.Dock = DockStyle.Top
        filterPanel.Height = 40
        filterPanel.Padding = New Padding(50, 5, 50, 5)

        Dim lblFilter As New Label()
        lblFilter.Text = "Transaction Type:"
        lblFilter.Font = New Font("Arial", 10, FontStyle.Bold)
        lblFilter.Width = 120
        lblFilter.Dock = DockStyle.Left
        lblFilter.TextAlign = ContentAlignment.MiddleLeft

        cmbTransactionType = New ComboBox()
        cmbTransactionType.Items.AddRange({"All", "Income", "Expense"})
        cmbTransactionType.SelectedIndex = 0
        cmbTransactionType.DropDownStyle = ComboBoxStyle.DropDownList
        cmbTransactionType.Width = 150
        cmbTransactionType.Dock = DockStyle.Left
        cmbTransactionType.Font = New Font("Arial", 9)

        filterPanel.Controls.Add(cmbTransactionType)
        filterPanel.Controls.Add(lblFilter)

        ' DataGridView container
        Dim dgvContainer As New Panel()
        dgvContainer.Dock = DockStyle.Fill
        dgvContainer.Padding = New Padding(50, 10, 50, 20)
        dgvContainer.BackColor = Color.White

        dgvRecent.Dock = DockStyle.Fill
        dgvContainer.Controls.Add(dgvRecent)

        recentPanel.Controls.Add(dgvContainer)
        recentPanel.Controls.Add(filterPanel)
        recentPanel.Controls.Add(lblRecent)
        mainLayout.Controls.Add(recentPanel, 0, 1)

        dashboardTab.Controls.Add(mainLayout)
        tabControl.TabPages.Add(dashboardTab)
    End Sub

    Private Function CreateColoredBox(title As String, value As String, backgroundColor As Color) As Panel
        Dim box As New Panel()
        box.Dock = DockStyle.Fill
        box.Margin = New Padding(5)
        box.BackColor = backgroundColor
        box.Padding = New Padding(10)

        ' Value label (centered in the middle)
        Dim valueLabel As New Label()
        valueLabel.Text = value
        valueLabel.ForeColor = Color.White
        valueLabel.Font = New Font("Arial", 24, FontStyle.Bold)
        valueLabel.Dock = DockStyle.Fill
        valueLabel.TextAlign = ContentAlignment.MiddleCenter

        ' Title label (on top)
        Dim titleLabel As New Label()
        titleLabel.Text = title
        titleLabel.ForeColor = Color.White
        titleLabel.Font = New Font("Arial", 9, FontStyle.Bold)
        titleLabel.Dock = DockStyle.Top
        titleLabel.Height = 25
        titleLabel.TextAlign = ContentAlignment.MiddleCenter

        ' Add controls to the box (value first, then title - due to dock order)
        box.Controls.Add(valueLabel)
        box.Controls.Add(titleLabel)

        Return box
    End Function
    Private Sub CreateIncomeTab()
        cmbIncomeSource = New ComboBox()
        cmbIncomeSource.DropDownStyle = ComboBoxStyle.DropDownList
        dgvBookings = New DataGridView()
        dgvBookings.Dock = DockStyle.Fill
        incomeTab = New TabPage("Income Management")
        incomeTab.BackColor = Color.White
        incomeTab.Padding = New Padding(10)

        ' Initialize splitContainer first
        Dim splitContainer As New SplitContainer()
        splitContainer.Dock = DockStyle.Fill
        splitContainer.Orientation = Orientation.Horizontal
        splitContainer.SplitterDistance = 400
        splitContainer.IsSplitterFixed = True

        ' Initialize top panel
        Dim topPanel As New GroupBox()
        topPanel.Text = "Available Bookings"
        topPanel.Dock = DockStyle.Fill
        topPanel.Padding = New Padding(10)

        ' Initialize DataGridView for bookings
        dgvBookings = New DataGridView()
        dgvBookings.Dock = DockStyle.Fill
        dgvBookings.ReadOnly = False ' Allow editing of checkboxes
        dgvBookings.AllowUserToAddRows = False
        dgvBookings.AllowUserToDeleteRows = False
        dgvBookings.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvBookings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvBookings.MultiSelect = True
        dgvBookings.RowHeadersVisible = False

        ' Add checkbox column FIRST (important for proper binding)
        Dim checkBoxColumn As New DataGridViewCheckBoxColumn()
        checkBoxColumn.Name = "Selected"
        checkBoxColumn.HeaderText = "Select"
        checkBoxColumn.Width = 50
        checkBoxColumn.TrueValue = True
        checkBoxColumn.FalseValue = False
        dgvBookings.Columns.Add(checkBoxColumn)

        ' Add other columns
        dgvBookings.Columns.Add("BookingID", "BookingID")
        dgvBookings.Columns.Add("Title", "Title")
        dgvBookings.Columns.Add("Status", "Status")
        dgvBookings.Columns.Add("Amount", "Amount")
        dgvBookings.Columns.Add("Date", "Date")
        AddHandler dgvBookings.CellContentClick, AddressOf dgvBookings_CellContentClick
        ' Initialize bottom panel
        Dim bottomPanel As New GroupBox()
        bottomPanel.Text = "Recorded Income"
        bottomPanel.Dock = DockStyle.Fill
        bottomPanel.Padding = New Padding(10)

        ' Initialize DataGridView for income
        dgvIncome = New DataGridView()
        dgvIncome.Dock = DockStyle.Fill
        dgvIncome.ReadOnly = True
        dgvIncome.AllowUserToAddRows = False
        dgvIncome.AllowUserToDeleteRows = False
        dgvIncome.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvIncome.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvIncome.RowHeadersVisible = False

        ' Initialize buttons
        Dim buttonPanel As New FlowLayoutPanel()
        buttonPanel.FlowDirection = FlowDirection.LeftToRight
        buttonPanel.Dock = DockStyle.Bottom
        buttonPanel.Height = 40
        buttonPanel.Padding = New Padding(0, 5, 0, 0)

        btnAddIncome = New Button()
        btnAddIncome.Text = "Add Selected as Income"
        btnAddIncome.Size = New Size(180, 30)
        btnAddIncome.BackColor = Color.FromArgb(40, 167, 69)
        btnAddIncome.ForeColor = Color.White
        btnAddIncome.FlatStyle = FlatStyle.Flat

        btnRefreshBookings = New Button()
        btnRefreshBookings.Text = "Refresh Bookings"
        btnRefreshBookings.Size = New Size(120, 30)
        btnRefreshBookings.BackColor = Color.FromArgb(0, 123, 255)
        btnRefreshBookings.ForeColor = Color.White
        btnRefreshBookings.FlatStyle = FlatStyle.Flat

        buttonPanel.Controls.AddRange({btnAddIncome, btnRefreshBookings})

        ' Add controls to panels
        topPanel.Controls.Add(dgvBookings)
        topPanel.Controls.Add(buttonPanel)
        bottomPanel.Controls.Add(dgvIncome)

        ' Add panels to split container
        splitContainer.Panel1.Controls.Add(topPanel)
        splitContainer.Panel2.Controls.Add(bottomPanel)

        ' Add split container to tab page
        incomeTab.Controls.Add(splitContainer)

        ' Add tab page to tab control
        tabControl.TabPages.Add(incomeTab)

        ' Add event handlers
        AddHandler btnAddIncome.Click, AddressOf btnAddIncome_Click
        AddHandler btnRefreshBookings.Click, AddressOf btnRefreshBookings_Click
    End Sub
    Private Sub btnRefreshBookings_Click(sender As Object, e As EventArgs)
        LoadBookings()
    End Sub
    Private Sub cmbIncomeSource_SelectedIndexChanged(sender As Object, e As EventArgs)
        LoadBookings()
    End Sub
    ' Add this new method to handle checkbox clicks
    Private Sub dgvBookings_CellContentClick(sender As Object, e As DataGridViewCellEventArgs)
        ' Only handle checkbox column clicks
        If e.ColumnIndex = dgvBookings.Columns("Selected").Index AndAlso e.RowIndex >= 0 Then
            dgvBookings.CommitEdit(DataGridViewDataErrorContexts.Commit)
        End If
    End Sub
    Private Sub CreateExpenseTab()
        expenseTab = New TabPage("Expense Management")
        expenseTab.BackColor = Color.White
        expenseTab.Padding = New Padding(10)

        ' Main split container
        Dim splitContainer As New SplitContainer()
        splitContainer.Dock = DockStyle.Fill
        splitContainer.Orientation = Orientation.Horizontal
        splitContainer.SplitterDistance = 300
        splitContainer.IsSplitterFixed = True

        ' Top panel for expense entry
        Dim topPanel As New GroupBox()
        topPanel.Text = "Add/Edit Expense (Select from Expense Items) - MONTHLY EXPENSES"
        topPanel.Dock = DockStyle.Fill
        topPanel.Padding = New Padding(10)

        Dim expenseLayout As New TableLayoutPanel()
        expenseLayout.ColumnCount = 3
        expenseLayout.RowCount = 6
        expenseLayout.Dock = DockStyle.Fill
        expenseLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33))
        expenseLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33))
        expenseLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 34))

        ' Expense Category
        expenseLayout.Controls.Add(New Label() With {.Text = "Expense Category:", .Anchor = AnchorStyles.Left}, 0, 0)
        cmbExpenseCategory = New ComboBox()
        cmbExpenseCategory.DropDownStyle = ComboBoxStyle.DropDownList
        cmbExpenseCategory.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        expenseLayout.Controls.Add(cmbExpenseCategory, 0, 1)

        ' Date
        expenseLayout.Controls.Add(New Label() With {.Text = "Date:", .Anchor = AnchorStyles.Left}, 1, 0)
        dtpExpenseDate = New DateTimePicker()
        dtpExpenseDate.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        expenseLayout.Controls.Add(dtpExpenseDate, 1, 1)



        ' Expense Items Selection
        expenseLayout.Controls.Add(New Label() With {.Text = "Select Expense Items:", .Anchor = AnchorStyles.Left}, 0, 2)
        lstExpenseBookingIds = New CheckedListBox()
        lstExpenseBookingIds.CheckOnClick = True
        lstExpenseBookingIds.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top Or AnchorStyles.Bottom
        expenseLayout.SetColumnSpan(lstExpenseBookingIds, 2)
        expenseLayout.Controls.Add(lstExpenseBookingIds, 0, 3)

        ' Computed Amount
        expenseLayout.Controls.Add(New Label() With {.Text = "Amount:", .Anchor = AnchorStyles.Left}, 2, 2)
        lblComputedExpenseAmount = New Label() With {.Text = "₱0.00", .Font = New Font("Arial", 10, FontStyle.Bold), .ForeColor = Color.Red, .Anchor = AnchorStyles.Left}
        expenseLayout.Controls.Add(lblComputedExpenseAmount, 2, 3)

        ' Description
        expenseLayout.Controls.Add(New Label() With {.Text = "Description:", .Anchor = AnchorStyles.Left}, 0, 4)
        txtExpenseDescription = New TextBox()
        txtExpenseDescription.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        expenseLayout.SetColumnSpan(txtExpenseDescription, 2)
        expenseLayout.Controls.Add(txtExpenseDescription, 0, 5)

        ' Receipt Number
        expenseLayout.Controls.Add(New Label() With {.Text = "Receipt #:", .Anchor = AnchorStyles.Left}, 2, 4)
        txtReceiptNumber = New TextBox()
        txtReceiptNumber.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        expenseLayout.Controls.Add(txtReceiptNumber, 2, 5)

        ' Buttons
        Dim buttonPanel As New FlowLayoutPanel()
        buttonPanel.FlowDirection = FlowDirection.LeftToRight
        buttonPanel.Dock = DockStyle.Bottom
        buttonPanel.Height = 40
        buttonPanel.Padding = New Padding(0, 5, 0, 0)

        btnAddExpense = New Button() With {.Text = "Add Expense", .Size = New Size(100, 30), .BackColor = Color.FromArgb(220, 53, 69), .ForeColor = Color.White, .FlatStyle = FlatStyle.Flat}
        btnEditExpense = New Button() With {.Text = "Edit", .Size = New Size(80, 30), .BackColor = Color.FromArgb(255, 193, 7), .ForeColor = Color.Black, .FlatStyle = FlatStyle.Flat}
        btnDeleteExpense = New Button() With {.Text = "Delete", .Size = New Size(80, 30), .BackColor = Color.FromArgb(108, 117, 125), .ForeColor = Color.White, .FlatStyle = FlatStyle.Flat}

        buttonPanel.Controls.AddRange({btnAddExpense, btnEditExpense, btnDeleteExpense})

        topPanel.Controls.Add(expenseLayout)
        topPanel.Controls.Add(buttonPanel)

        ' Bottom panel for expense list
        Dim bottomPanel As New GroupBox()
        bottomPanel.Text = "Expense Records"
        bottomPanel.Dock = DockStyle.Fill
        bottomPanel.Padding = New Padding(10)

        dgvExpenses = New DataGridView()
        dgvExpenses.Dock = DockStyle.Fill
        dgvExpenses.ReadOnly = True
        dgvExpenses.AllowUserToAddRows = False
        dgvExpenses.AllowUserToDeleteRows = False
        dgvExpenses.AllowUserToResizeColumns = False
        dgvExpenses.AllowUserToResizeRows = False
        dgvExpenses.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvExpenses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvExpenses.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        dgvExpenses.RowHeadersVisible = False
        dgvExpenses.MultiSelect = False

        bottomPanel.Controls.Add(dgvExpenses)

        splitContainer.Panel1.Controls.Add(topPanel)
        splitContainer.Panel2.Controls.Add(bottomPanel)

        expenseTab.Controls.Add(splitContainer)
        tabControl.TabPages.Add(expenseTab)

        ' Add event handlers
        AddHandler cmbExpenseCategory.SelectedIndexChanged, AddressOf cmbExpenseCategory_SelectedIndexChanged
        AddHandler lstExpenseBookingIds.ItemCheck, AddressOf lstExpenseBookingIds_ItemCheck
    End Sub

    Private Sub cmbExpenseCategory_SelectedIndexChanged(sender As Object, e As EventArgs)
        LoadExpenseItemsByCategory()
    End Sub

    Private Sub CreateReportsTab()
        reportsTab = New TabPage("Financial Reports")
        reportsTab.BackColor = Color.White
        reportsTab.Padding = New Padding(5)

        ' Main container
        Dim mainContainer As New Panel()
        mainContainer.Dock = DockStyle.Fill

        ' Filter panel at top - INCREASED HEIGHT
        Dim filterPanel As New Panel()
        filterPanel.Height = 120  ' Increased from 80
        filterPanel.Dock = DockStyle.Top
        filterPanel.BackColor = Color.FromArgb(248, 249, 250)
        filterPanel.Padding = New Padding(15, 10, 15, 10)  ' More padding

        ' Filter controls in a single row
        Dim flowPanel As New FlowLayoutPanel()
        flowPanel.FlowDirection = FlowDirection.LeftToRight
        flowPanel.Dock = DockStyle.Fill
        flowPanel.WrapContents = False





        ' Start Date Label and DateTimePicker
        Dim lblStart As New Label()
        lblStart.Text = "From:"
        lblStart.Font = New Font("Arial", 10)
        lblStart.TextAlign = ContentAlignment.MiddleLeft
        lblStart.Width = 45
        lblStart.Height = 20
        lblStart.Margin = New Padding(15, 15, 5, 5)

        dtpReportStart = New DateTimePicker()
        dtpReportStart.Width = 120
        dtpReportStart.Height = 30
        dtpReportStart.Font = New Font("Arial", 9)
        dtpReportStart.Format = DateTimePickerFormat.Short  ' Short date format
        dtpReportStart.Margin = New Padding(15, 15, 15, 5)

        ' End Date Label and DateTimePicker
        Dim lblEnd As New Label()
        lblEnd.Text = "To:"
        lblEnd.Font = New Font("Arial", 10)
        lblEnd.TextAlign = ContentAlignment.MiddleLeft
        lblEnd.Width = 25
        lblEnd.Height = 30
        lblEnd.Margin = New Padding(15, 15, 5, 5)

        dtpReportEnd = New DateTimePicker()
        dtpReportEnd.Width = 120
        dtpReportEnd.Height = 30
        dtpReportEnd.Font = New Font("Arial", 9)
        dtpReportEnd.Format = DateTimePickerFormat.Short  ' Short date format
        dtpReportEnd.Margin = New Padding(15, 15, 15, 5)

        btnGenerateReport = New Button()
        btnGenerateReport.Text = "Generate Report"
        btnGenerateReport.Width = 150
        btnGenerateReport.Height = 35
        btnGenerateReport.Font = New Font("Arial", 10, FontStyle.Bold)
        btnGenerateReport.BackColor = Color.FromArgb(0, 123, 255)
        btnGenerateReport.ForeColor = Color.White
        btnGenerateReport.FlatStyle = FlatStyle.Flat
        btnGenerateReport.Margin = New Padding(10, 13, 5, 5)

        btnExportReport = New Button()
        btnExportReport.Text = "Export to Excel"
        btnExportReport.Width = 150
        btnExportReport.Height = 35
        btnExportReport.Font = New Font("Arial", 10, FontStyle.Bold)
        btnExportReport.BackColor = Color.FromArgb(40, 167, 69)
        btnExportReport.ForeColor = Color.White
        btnExportReport.FlatStyle = FlatStyle.Flat
        btnExportReport.Margin = New Padding(10, 13, 5, 5)

        flowPanel.Controls.AddRange({lblStart, dtpReportStart, lblEnd, dtpReportEnd, btnGenerateReport, btnExportReport})
        filterPanel.Controls.Add(flowPanel)

        ' Summary panel - INCREASED HEIGHT
        Dim summaryPanel As New Panel()
        summaryPanel.Height = 100  ' Increased to accommodate headers
        summaryPanel.Dock = DockStyle.Top
        summaryPanel.BackColor = Color.FromArgb(248, 249, 250)
        summaryPanel.Padding = New Padding(15, 10, 15, 10)

        ' Create a TableLayoutPanel for even distribution with 2 rows
        Dim summaryTable As New TableLayoutPanel()
        summaryTable.Dock = DockStyle.Fill
        summaryTable.ColumnCount = 4
        summaryTable.RowCount = 2
        summaryTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        summaryTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        summaryTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        summaryTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        summaryTable.RowStyles.Add(New RowStyle(SizeType.Percent, 40.0F))  ' Header row
        summaryTable.RowStyles.Add(New RowStyle(SizeType.Percent, 60.0F))  ' Value row

        ' Header labels
        Dim lblIncomeHeader As New Label()
        lblIncomeHeader.Text = "Total Income"
        lblIncomeHeader.Font = New Font("Arial", 16, FontStyle.Bold)
        lblIncomeHeader.ForeColor = Color.FromArgb(40, 167, 69)
        lblIncomeHeader.TextAlign = ContentAlignment.MiddleCenter
        lblIncomeHeader.Dock = DockStyle.Fill

        Dim lblExpensesHeader As New Label()
        lblExpensesHeader.Text = "Total Expenses"
        lblExpensesHeader.Font = New Font("Arial", 16, FontStyle.Bold)
        lblExpensesHeader.ForeColor = Color.FromArgb(220, 53, 69)
        lblExpensesHeader.TextAlign = ContentAlignment.MiddleCenter
        lblExpensesHeader.Dock = DockStyle.Fill

        Dim lblProfitHeader As New Label()
        lblProfitHeader.Text = "Net Profit"
        lblProfitHeader.Font = New Font("Arial", 16, FontStyle.Bold)
        lblProfitHeader.ForeColor = Color.FromArgb(0, 123, 255)
        lblProfitHeader.TextAlign = ContentAlignment.MiddleCenter
        lblProfitHeader.Dock = DockStyle.Fill

        Dim lblMarginHeader As New Label()
        lblMarginHeader.Text = "Profit Margin"
        lblMarginHeader.Font = New Font("Arial", 16, FontStyle.Bold)
        lblMarginHeader.ForeColor = Color.FromArgb(108, 117, 125)
        lblMarginHeader.TextAlign = ContentAlignment.MiddleCenter
        lblMarginHeader.Dock = DockStyle.Fill

        ' Value labels
        lblReportIncome = New Label()
        lblReportIncome.Text = "₱0.00"
        lblReportIncome.Font = New Font("Arial", 12, FontStyle.Bold)
        lblReportIncome.ForeColor = Color.FromArgb(40, 167, 69)
        lblReportIncome.TextAlign = ContentAlignment.MiddleCenter
        lblReportIncome.Dock = DockStyle.Fill

        lblReportExpenses = New Label()
        lblReportExpenses.Text = "₱0.00"
        lblReportExpenses.Font = New Font("Arial", 12, FontStyle.Bold)
        lblReportExpenses.ForeColor = Color.FromArgb(220, 53, 69)
        lblReportExpenses.TextAlign = ContentAlignment.MiddleCenter
        lblReportExpenses.Dock = DockStyle.Fill

        lblReportProfit = New Label()
        lblReportProfit.Text = "₱0.00"
        lblReportProfit.Font = New Font("Arial", 12, FontStyle.Bold)
        lblReportProfit.ForeColor = Color.FromArgb(0, 123, 255)
        lblReportProfit.TextAlign = ContentAlignment.MiddleCenter
        lblReportProfit.Dock = DockStyle.Fill

        lblReportMargin = New Label()
        lblReportMargin.Text = "0%"
        lblReportMargin.Font = New Font("Arial", 12, FontStyle.Bold)
        lblReportMargin.ForeColor = Color.FromArgb(108, 117, 125)
        lblReportMargin.TextAlign = ContentAlignment.MiddleCenter
        lblReportMargin.Dock = DockStyle.Fill

        ' Add headers to first row
        summaryTable.Controls.Add(lblIncomeHeader, 0, 0)
        summaryTable.Controls.Add(lblExpensesHeader, 1, 0)
        summaryTable.Controls.Add(lblProfitHeader, 2, 0)
        summaryTable.Controls.Add(lblMarginHeader, 3, 0)

        ' Add values to second row
        summaryTable.Controls.Add(lblReportIncome, 0, 1)
        summaryTable.Controls.Add(lblReportExpenses, 1, 1)
        summaryTable.Controls.Add(lblReportProfit, 2, 1)
        summaryTable.Controls.Add(lblReportMargin, 3, 1)

        summaryPanel.Controls.Add(summaryTable)

        dgvReport = New DataGridView()
        dgvReport.Dock = DockStyle.Fill
        dgvReport.ReadOnly = True
        dgvReport.AllowUserToAddRows = False
        dgvReport.AllowUserToDeleteRows = False
        dgvReport.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvReport.ColumnHeadersHeight = 35  ' Slightly larger headers
        dgvReport.RowHeadersVisible = False
        dgvReport.BackgroundColor = Color.White
        dgvReport.BorderStyle = BorderStyle.FixedSingle
        dgvReport.Font = New Font("Arial", 10)  ' Larger font
        dgvReport.RowTemplate.Height = 30  ' Taller rows
        dgvReport.ScrollBars = ScrollBars.Both
        dgvReport.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 58, 64)
        dgvReport.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        dgvReport.ColumnHeadersDefaultCellStyle.Font = New Font("Arial", 10, FontStyle.Bold)  ' Larger header font

        ' Add a margin around the DataGridView
        dgvReport.Margin = New Padding(10)

        mainContainer.Controls.Add(dgvReport)
        mainContainer.Controls.Add(summaryPanel)
        mainContainer.Controls.Add(filterPanel)

        reportsTab.Controls.Add(mainContainer)
        tabControl.TabPages.Add(reportsTab)
    End Sub

    Private Sub LoadData()
        LoadIncomeSources()
        LoadExpenseCategories()
        LoadBookings()
        LoadIncomeData()
        LoadExpenseData()
    End Sub


    Private Sub LoadBookings()
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim query As String = "SELECT b.BookingID, tp.Title, b.Status, " &
                              "ba.FinalAmount as Amount, b.BookingDate as Date " &
                              "FROM Bookings b " &
                              "INNER JOIN TourPackages tp ON b.PackageID = tp.PackageID " &
                              "INNER JOIN BookingAmounts ba ON b.BookingID = ba.BookingID " &
                              "WHERE b.Status IN ('Confirmed', 'Completed') " &
                              "AND b.BookingID NOT IN (SELECT BookingID FROM Income WHERE BookingID IS NOT NULL) " &
                              "ORDER BY b.BookingDate DESC"

                Using cmd As New MySqlCommand(query, conn)
                    Using adapter As New MySqlDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        ' Clear existing rows but keep columns
                        dgvBookings.Rows.Clear()

                        ' Add rows to the DataGridView
                        For Each row As DataRow In dt.Rows
                            Dim index As Integer = dgvBookings.Rows.Add()
                            ' Initialize checkbox to False and ensure proper type
                            dgvBookings.Rows(index).Cells("Selected").Value = False
                            dgvBookings.Rows(index).Cells("BookingID").Value = row("BookingID")
                            dgvBookings.Rows(index).Cells("Title").Value = row("Title")
                            dgvBookings.Rows(index).Cells("Status").Value = row("Status")

                            ' Format amount as decimal first, then format as currency
                            Dim amount As Decimal = Convert.ToDecimal(row("Amount"))
                            dgvBookings.Rows(index).Cells("Amount").Value = FinancialHelpers.FormatAsCurrency(amount)

                            dgvBookings.Rows(index).Cells("Date").Value = Convert.ToDateTime(row("Date")).ToString("yyyy-MM-dd")
                        Next
                    End Using
                End Using
                conn.Close()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading bookings: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LoadIncomeSources()
        Try
            cmbIncomeSource.Items.Clear() ' Clear before loading

            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim query As String = "SELECT IncomeSourceID, Category FROM IncomeSources WHERE IsActive = 1"
                Using cmd As New MySqlCommand(query, conn)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.HasRows Then
                            While reader.Read()
                                cmbIncomeSource.Items.Add(New With {
                                .Value = reader("IncomeSourceID"),
                                .Text = reader("Category").ToString()
                            })
                            End While

                            cmbIncomeSource.DisplayMember = "Text"
                            cmbIncomeSource.ValueMember = "Value"
                        Else
                            ' Add default items if none in database
                            cmbIncomeSource.Items.Add(New With {.Value = 1, .Text = "Tour Bookings"})
                            cmbIncomeSource.Items.Add(New With {.Value = 2, .Text = "Other Income"})
                        End If
                    End Using
                End Using
                conn.Close()
            End Using

            ' Set default selection if items exist
            If cmbIncomeSource.Items.Count > 0 Then
                cmbIncomeSource.SelectedIndex = 0
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading income sources: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LoadExpenseCategories()
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim query As String = "SELECT ExpenseCategoryID, Category FROM ExpenseCategories ORDER BY ExpenseCategoryID"
                Using cmd As New MySqlCommand(query, conn)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        cmbExpenseCategory.Items.Clear()
                        While reader.Read()
                            cmbExpenseCategory.Items.Add(New With {.Value = reader("ExpenseCategoryID"), .Text = reader("Category").ToString()})
                        End While
                    End Using
                End Using
                conn.Close()
            End Using
            cmbExpenseCategory.DisplayMember = "Text"
            cmbExpenseCategory.ValueMember = "Value"

            ' Set default selection
            If cmbExpenseCategory.Items.Count > 0 Then
                cmbExpenseCategory.SelectedIndex = 0
            End If

        Catch ex As Exception
            MessageBox.Show("Error loading expense categories: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LoadExpenseItemsByCategory()
        Try
            If cmbExpenseCategory.SelectedIndex = -1 Then
                lstExpenseBookingIds.Items.Clear()
                Return
            End If

            Dim selectedCategory As String = CType(cmbExpenseCategory.SelectedItem, Object).Text
            lstExpenseBookingIds.Items.Clear()

            ' Define expense items for each category
            Select Case selectedCategory
                Case "Technology & Website"
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Website Hosting & Domain", .EstimatedAmount = 2000, .CategoryType = "Operating"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Booking System Software", .EstimatedAmount = 8000, .CategoryType = "Operating"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "CRM/Customer Management", .EstimatedAmount = 3000, .CategoryType = "Operating"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Payment Gateway Fees", .EstimatedAmount = 1500, .CategoryType = "Operating"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Website Maintenance", .EstimatedAmount = 5000, .CategoryType = "Operating"})
                Case "Startup Expenses"
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Business Registration", .EstimatedAmount = 5000, .CategoryType = "Startup"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Website Development", .EstimatedAmount = 30000, .CategoryType = "Startup"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Office Equipment", .EstimatedAmount = 60000, .CategoryType = "Startup"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Initial Marketing", .EstimatedAmount = 15000, .CategoryType = "Startup"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Legal Fees", .EstimatedAmount = 12000, .CategoryType = "Startup"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Insurance", .EstimatedAmount = 10000, .CategoryType = "Startup"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Initial Inventory", .EstimatedAmount = 20000, .CategoryType = "Startup"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Training Materials", .EstimatedAmount = 5000, .CategoryType = "Startup"})

                Case "Content Creation & SEO"
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Content Creation", .EstimatedAmount = 10000, .CategoryType = "Operating"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "SEO Tools & Services", .EstimatedAmount = 7000, .CategoryType = "Operating"})

                Case "Business Operations"
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Internet Connection", .EstimatedAmount = 2500, .CategoryType = "Operating"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Phone/Mobile Plans", .EstimatedAmount = 1500, .CategoryType = "Operating"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Office Supplies", .EstimatedAmount = 3000, .CategoryType = "Operating"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Electricity", .EstimatedAmount = 4000, .CategoryType = "Operating"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Professional Fees", .EstimatedAmount = 15000, .CategoryType = "Operating"})

                Case "Legal & Compliance"
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Business Insurance", .EstimatedAmount = 20000, .CategoryType = "Operating"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "License Renewal", .EstimatedAmount = 8000, .CategoryType = "Operating"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Legal Consultation", .EstimatedAmount = 12000, .CategoryType = "Operating"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Tax Compliance", .EstimatedAmount = 10000, .CategoryType = "Operating"})

                Case "Personal Development"
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Training & Certifications", .EstimatedAmount = 15000, .CategoryType = "Operating"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Industry Conferences", .EstimatedAmount = 25000, .CategoryType = "Operating"})
                    lstExpenseBookingIds.Items.Add(New ExpenseItem With {.ItemName = "Travel for FAM Trips", .EstimatedAmount = 30000, .CategoryType = "Operating"})
            End Select

            lstExpenseBookingIds.DisplayMember = "DisplayText"

        Catch ex As Exception
            MessageBox.Show("Error loading expense items: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LoadIncomeData()
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim query As String = "SELECT 
                i.IncomeID, 
                i.IncomeDate, 
                i.Description, 
                ba.FinalAmount AS GrossAmount,  -- Get from BookingAmounts
                (ba.FinalAmount * 0.2) AS CommissionAmount,  -- Calculate 20%
                (ba.FinalAmount * 0.8) AS NetAmount,  -- Calculate 80%
                i.BookingID 
            FROM Income i
            LEFT JOIN Bookings b ON i.BookingID = b.BookingID
            LEFT JOIN BookingAmounts ba ON b.BookingID = ba.BookingID
            ORDER BY i.IncomeDate DESC"

                Using cmd As New MySqlCommand(query, conn)
                    Using adapter As New MySqlDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        ' Add formatted columns
                        dt.Columns.Add("FormattedGross", GetType(String))
                        dt.Columns.Add("FormattedCommission", GetType(String))
                        dt.Columns.Add("FormattedNet", GetType(String))
                        dt.Columns.Add("FormattedDate", GetType(String))

                        For Each row As DataRow In dt.Rows
                            row("FormattedGross") = FinancialHelpers.FormatAsCurrency(Convert.ToDecimal(row("GrossAmount")))
                            row("FormattedCommission") = FinancialHelpers.FormatAsCurrency(Convert.ToDecimal(row("CommissionAmount")))
                            row("FormattedNet") = FinancialHelpers.FormatAsCurrency(Convert.ToDecimal(row("NetAmount")))
                            row("FormattedDate") = Convert.ToDateTime(row("IncomeDate")).ToString("yyyy-MM-dd")
                        Next

                        dgvIncome.DataSource = dt
                        ' Configure column visibility and headers...
                    End Using
                End Using
                conn.Close()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading income data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    ' Replace the LoadExpenseData method completely:
    Private Sub LoadExpenseData()
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim query As String = "SELECT e.ExpenseID, e.ExpenseDate, ec.Category, e.ExpenseItem, e.Description, " &
                          "e.Amount, e.ReceiptNumber, e.ExpenseType " &
                          "FROM Expenses e " &
                          "INNER JOIN ExpenseCategories ec ON e.ExpenseCategoryID = ec.ExpenseCategoryID " &
                          "ORDER BY e.ExpenseDate DESC"

                Using cmd As New MySqlCommand(query, conn)
                    Using adapter As New MySqlDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        ' Add formatted columns for display
                        dt.Columns.Add("FormattedAmount", GetType(String))
                        dt.Columns.Add("FormattedDate", GetType(String))

                        For Each row As DataRow In dt.Rows
                            row("FormattedAmount") = "₱" & Convert.ToDecimal(row("Amount")).ToString("N2")
                            row("FormattedDate") = Convert.ToDateTime(row("ExpenseDate")).ToString("yyyy-MM-dd")
                        Next

                        ' Hide raw columns and show formatted ones
                        dgvExpenses.DataSource = dt
                        dgvExpenses.Columns("Amount").Visible = False
                        dgvExpenses.Columns("ExpenseDate").Visible = False

                        dgvExpenses.Columns("FormattedAmount").HeaderText = "Amount"
                        dgvExpenses.Columns("FormattedDate").HeaderText = "Date"
                        dgvExpenses.Columns("ExpenseItem").HeaderText = "Expense Items"
                    End Using
                End Using
                conn.Close()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading expense data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LoadDashboardData()
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()

                ' Calculate totals directly from database
                Dim totalGrossIncome As Decimal = 0
                Dim totalCommissionIncome As Decimal = 0
                Dim totalExpenses As Decimal = 0

                ' Get total gross income and commission from bookings
                Dim incomeQuery As String = "SELECT " &
            "COALESCE(SUM(ba.FinalAmount), 0) as GrossIncome, " &
            "COALESCE(SUM(ba.FinalAmount * 0.2), 0) as CommissionIncome " &
            "FROM Bookings b " &
            "JOIN BookingAmounts ba ON b.BookingID = ba.BookingID " &
            "WHERE b.Status IN ('Confirmed', 'Completed')"

                Using cmd As New MySqlCommand(incomeQuery, conn)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            totalGrossIncome = Convert.ToDecimal(reader("GrossIncome"))
                            totalCommissionIncome = Convert.ToDecimal(reader("CommissionIncome"))
                        End If
                    End Using
                End Using

                ' Get total expenses directly
                Dim expenseQuery As String = "SELECT COALESCE(SUM(Amount), 0) FROM Expenses"
                Using cmd As New MySqlCommand(expenseQuery, conn)
                    totalExpenses = Convert.ToDecimal(cmd.ExecuteScalar())
                End Using

                ' Update dashboard with calculated values
                UpdateDashboardLabels(totalGrossIncome, totalCommissionIncome, totalExpenses)

                ' Load recent transactions based on current filter
                Dim selectedType As String = If(cmbTransactionType.SelectedItem IsNot Nothing, cmbTransactionType.SelectedItem.ToString(), "All")
                LoadRecentTransactions(selectedType)

                conn.Close()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading dashboard data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub UpdateDashboardLabels(grossIncome As Decimal, commissionIncome As Decimal, totalExpenses As Decimal)
        ' Calculate values in real-time
        Dim netProfit As Decimal = commissionIncome - totalExpenses
        Dim profitPercentage As Decimal = If(commissionIncome > 0, (netProfit / commissionIncome) * 100, 0)

        ' Update UI
        lblTotalIncome.Text = FinancialHelpers.FormatAsCurrency(grossIncome)
        lblCommissionIncome.Text = FinancialHelpers.FormatAsCurrency(commissionIncome)
        lblTotalExpenses.Text = FinancialHelpers.FormatAsCurrency(totalExpenses)
        lblNetProfit.Text = FinancialHelpers.FormatAsCurrency(netProfit)
        lblProfitMargin.Text = profitPercentage.ToString("N1") & "%"
    End Sub
    Private Sub FinancialManagementForms_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            ' Initialize all components first
            InitializeDashboardComponents()
            InitializeRecentTransactionsGrid()

            ' Then load data
            LoadDashboardData()

            ' Set default report date range (current month)
            dtpReportStart.Value = New DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
            dtpReportEnd.Value = DateTime.Now.Date

        Catch ex As Exception
            MessageBox.Show($"Error loading financial form: {ex.Message}", "Loading Error",
                       MessageBoxButtons.OK, MessageBoxIcon.Error)
#If DEBUG Then
            MessageBox.Show($"Stack Trace: {ex.StackTrace}", "Debug Info",
                       MessageBoxButtons.OK, MessageBoxIcon.Information)
#End If
        End Try
    End Sub
    Private Sub SetupIncomeColumns()
        dgvRecent.Columns.Clear()
        dgvRecent.Columns.Add("Description", "Description")
        dgvRecent.Columns.Add("Amount", "Amount")
        dgvRecent.Columns.Add("Date", "Date")
        dgvRecent.Columns.Add("UserID", "User ID")
        dgvRecent.Columns.Add("BookingID", "Booking ID")
        dgvRecent.Columns.Add("PackageCategory", "Package Category")
        dgvRecent.Columns.Add("BookingDate", "Booking Date")
        dgvRecent.Columns.Add("PaymentMethod", "Payment Method")

        ' Set column widths
        dgvRecent.Columns("Description").FillWeight = 25
        dgvRecent.Columns("Amount").FillWeight = 15
        dgvRecent.Columns("Date").FillWeight = 12
        dgvRecent.Columns("UserID").FillWeight = 10
        dgvRecent.Columns("BookingID").FillWeight = 10
        dgvRecent.Columns("PackageCategory").FillWeight = 15
        dgvRecent.Columns("BookingDate").FillWeight = 12
        dgvRecent.Columns("PaymentMethod").FillWeight = 13
    End Sub

    Private Sub SetupExpenseColumns()
        dgvRecent.Columns.Clear()
        dgvRecent.Columns.Add("ExpenseName", "Expense Name")
        dgvRecent.Columns.Add("ExpenseCategory", "Expense Category")
        dgvRecent.Columns.Add("Description", "Description")
        dgvRecent.Columns.Add("Amount", "Amount")
        dgvRecent.Columns.Add("Date", "Date")
        dgvRecent.Columns.Add("PaymentMethod", "Payment Method")
        dgvRecent.Columns.Add("ReceiptNumber", "Receipt Number")
        dgvRecent.Columns.Add("AdminName", "Admin Name")

        ' Set column widths
        dgvRecent.Columns("ExpenseName").FillWeight = 18
        dgvRecent.Columns("ExpenseCategory").FillWeight = 15
        dgvRecent.Columns("Description").FillWeight = 20
        dgvRecent.Columns("Amount").FillWeight = 12
        dgvRecent.Columns("Date").FillWeight = 10
        dgvRecent.Columns("PaymentMethod").FillWeight = 12
        dgvRecent.Columns("ReceiptNumber").FillWeight = 10
        dgvRecent.Columns("AdminName").FillWeight = 13
    End Sub

    Private Sub SetupAllColumns()
        Try
            If dgvRecent Is Nothing Then Return

            dgvRecent.Columns.Clear()
            dgvRecent.Columns.Add("Type", "Type")
            dgvRecent.Columns.Add("Description", "Description")
            dgvRecent.Columns.Add("FormattedAmount", "Amount")
            dgvRecent.Columns.Add("FormattedDate", "Date")
            dgvRecent.Columns.Add("Details", "Details")

            ' Set column properties
            dgvRecent.Columns("Type").Width = 80
            dgvRecent.Columns("Type").AutoSizeMode = DataGridViewAutoSizeColumnMode.None

            dgvRecent.Columns("Description").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            dgvRecent.Columns("Description").FillWeight = 40

            dgvRecent.Columns("FormattedAmount").Width = 120
            dgvRecent.Columns("FormattedAmount").AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            dgvRecent.Columns("FormattedAmount").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight

            dgvRecent.Columns("FormattedDate").Width = 100
            dgvRecent.Columns("FormattedDate").AutoSizeMode = DataGridViewAutoSizeColumnMode.None

            dgvRecent.Columns("Details").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            dgvRecent.Columns("Details").FillWeight = 30

        Catch ex As Exception
            ' Handle silently
        End Try
    End Sub

    Private Sub EnsureColumnsInitialized()
        Try
            If dgvRecent IsNot Nothing AndAlso dgvRecent.Columns.Count = 0 Then
                SetupAllColumns()
            End If
        Catch ex As Exception
            ' Handle silently
        End Try
    End Sub

    Private Sub LoadRecentTransactions(transactionType As String)
        Try
            ' First ensure controls are initialized
            If dgvRecent Is Nothing OrElse cmbTransactionType Is Nothing Then
                InitializeAllControls()
            End If

            Using conn As New MySqlConnection(connectionString)
                conn.Open()

                Dim query As String = "SELECT 'Income' AS Type, i.Description, " &
                           "(ba.FinalAmount * 0.2) AS Amount, i.IncomeDate AS TransactionDate, " &
                           "CONCAT('Booking ID: ', COALESCE(i.BookingID, 'N/A'), ' | Commission: 20%') AS Details " &
                           "FROM Income i " &
                           "LEFT JOIN Bookings b ON i.BookingID = b.BookingID " &
                           "LEFT JOIN BookingAmounts ba ON b.BookingID = ba.BookingID " &
                           "WHERE @TransactionType IN ('All', 'Income') " &
                           "UNION ALL " &
                           "SELECT 'Expense' AS Type, e.Description, e.Amount, " &
                           "e.ExpenseDate AS TransactionDate, " &
                           "CONCAT('Category: ', ec.Category, ' | Receipt: ', COALESCE(e.ReceiptNumber, 'N/A')) AS Details " &
                           "FROM Expenses e " &
                           "LEFT JOIN ExpenseCategories ec ON e.ExpenseCategoryID = ec.ExpenseCategoryID " &
                           "WHERE @TransactionType IN ('All', 'Expense') " &
                           "ORDER BY TransactionDate DESC LIMIT 20"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@TransactionType", transactionType)

                    Using adapter As New MySqlDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        ' Add formatted columns for display
                        dt.Columns.Add("FormattedAmount", GetType(String))
                        dt.Columns.Add("FormattedDate", GetType(String))

                        ' Format the data
                        For Each row As DataRow In dt.Rows
                            ' Format amount as currency
                            Dim amount As Decimal = Convert.ToDecimal(row("Amount"))
                            row("FormattedAmount") = FinancialHelpers.FormatAsCurrency(amount)

                            ' Format date
                            Dim transactionDate As DateTime = Convert.ToDateTime(row("TransactionDate"))
                            row("FormattedDate") = transactionDate.ToString("yyyy-MM-dd")
                        Next

                        ' Clear existing columns and recreate them to match data
                        dgvRecent.DataSource = Nothing
                        dgvRecent.Columns.Clear()

                        ' Set the data source first
                        dgvRecent.DataSource = dt

                        ' Now configure the columns after binding
                        If dgvRecent.Columns.Contains("Amount") Then
                            dgvRecent.Columns("Amount").Visible = False
                        End If
                        If dgvRecent.Columns.Contains("TransactionDate") Then
                            dgvRecent.Columns("TransactionDate").Visible = False
                        End If

                        ' Set proper headers for visible columns
                        If dgvRecent.Columns.Contains("Type") Then
                            dgvRecent.Columns("Type").HeaderText = "Type"
                            dgvRecent.Columns("Type").Width = 80
                        End If
                        If dgvRecent.Columns.Contains("Description") Then
                            dgvRecent.Columns("Description").HeaderText = "Description"
                            dgvRecent.Columns("Description").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                            dgvRecent.Columns("Description").FillWeight = 40
                        End If
                        If dgvRecent.Columns.Contains("FormattedAmount") Then
                            dgvRecent.Columns("FormattedAmount").HeaderText = "Amount"
                            dgvRecent.Columns("FormattedAmount").Width = 120
                            dgvRecent.Columns("FormattedAmount").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
                        End If
                        If dgvRecent.Columns.Contains("FormattedDate") Then
                            dgvRecent.Columns("FormattedDate").HeaderText = "Date"
                            dgvRecent.Columns("FormattedDate").Width = 100
                        End If
                        If dgvRecent.Columns.Contains("Details") Then
                            dgvRecent.Columns("Details").HeaderText = "Details"
                            dgvRecent.Columns("Details").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                            dgvRecent.Columns("Details").FillWeight = 30
                        End If

                        ' Apply alternating row colors for better readability
                        dgvRecent.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245)

                        ' Set row colors based on transaction type
                        For Each row As DataGridViewRow In dgvRecent.Rows
                            If row.Cells("Type").Value IsNot Nothing Then
                                Dim rowType As String = row.Cells("Type").Value.ToString()
                                If rowType = "Income" Then
                                    row.DefaultCellStyle.ForeColor = Color.FromArgb(40, 167, 69) ' Green for income
                                ElseIf rowType = "Expense" Then
                                    row.DefaultCellStyle.ForeColor = Color.FromArgb(220, 53, 69) ' Red for expenses
                                End If
                            End If
                        Next

                    End Using
                End Using
                conn.Close()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading transactions: " & ex.Message, "Error",
                   MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub cmbTransactionType_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbTransactionType.SelectedIndexChanged
        Try
            If cmbTransactionType.SelectedItem Is Nothing Then Return

            Dim selectedType As String = cmbTransactionType.SelectedItem.ToString()

            Select Case selectedType
                Case "Income"
                    SetupIncomeColumns()
                Case "Expense"
                    SetupExpenseColumns()
                Case "All"
                    SetupAllColumns()
            End Select

            LoadRecentTransactions(selectedType)
        Catch ex As Exception
            ' Handle error silently or show minimal error
        End Try
    End Sub
    Private Sub InitializeAllControls()
        ' Initialize dashboard controls
        If dgvRecent Is Nothing Then
            dgvRecent = New DataGridView()
            With dgvRecent
                .Dock = DockStyle.Fill
                .ReadOnly = True
                .AllowUserToAddRows = False
                .AllowUserToDeleteRows = False
                .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            End With
        End If

        If cmbTransactionType Is Nothing Then
            cmbTransactionType = New ComboBox()
            With cmbTransactionType
                .Items.AddRange({"All", "Income", "Expense"})
                .SelectedIndex = 0
                .DropDownStyle = ComboBoxStyle.DropDownList
            End With
        End If

        ' Initialize other controls similarly
        If dgvIncome Is Nothing Then dgvIncome = New DataGridView()
        If dgvExpenses Is Nothing Then dgvExpenses = New DataGridView()
        ' Add initialization for all other controls
    End Sub
    Private Sub InitializeRecentTransactionsGrid()
        Try
            ' Safely initialize DataGridView if null
            If dgvRecent Is Nothing Then
                dgvRecent = New DataGridView()
                With dgvRecent
                    .Dock = DockStyle.Fill
                    .ReadOnly = True
                    .AllowUserToAddRows = False
                    .AllowUserToDeleteRows = False
                    .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                    .ColumnHeadersHeight = 35
                    .RowTemplate.Height = 30
                End With
            End If

            ' Safely initialize ComboBox if null
            If cmbTransactionType Is Nothing Then
                cmbTransactionType = New ComboBox()
                With cmbTransactionType
                    .Items.AddRange({"All", "Income", "Expense"})
                    .SelectedIndex = 0
                    .DropDownStyle = ComboBoxStyle.DropDownList
                End With
            End If

            ' Clear existing columns safely
            If dgvRecent.Columns.Count > 0 Then
                dgvRecent.Columns.Clear()
            End If

            ' Setup columns with null checks
            SetupAllColumns()

            ' Remove and re-add event handler to prevent duplicates
            RemoveHandler cmbTransactionType.SelectedIndexChanged, AddressOf cmbTransactionType_SelectedIndexChanged
            AddHandler cmbTransactionType.SelectedIndexChanged, AddressOf cmbTransactionType_SelectedIndexChanged

            ' Load initial data with error handling
            LoadRecentTransactions("All")

        Catch ex As Exception
            MessageBox.Show($"Error initializing transactions grid: {ex.Message}", "Initialization Error",
                       MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub lstBookingIds_ItemCheck(sender As Object, e As ItemCheckEventArgs)
        ' Get the clicked item
        Dim clickedItem = lstBookingIds.Items(e.Index)

        ' Check if it's a BookingItem
        If TypeOf clickedItem Is BookingItem Then
            Dim bookingItem As BookingItem = CType(clickedItem, BookingItem)

            ' Check if it's a date separator (bookingID = -2 or contains "──")
            If bookingItem.bookingID = -2 Or bookingItem.DisplayText.Contains("──") Then
                ' This is a date separator - toggle all bookings for this date
                Dim dateStr As String = bookingItem.BookingDate.ToString("yyyy-MM-dd")
                Dim shouldCheck As Boolean = (e.NewValue = CheckState.Checked)

                ' Prevent the date separator itself from being checked
                e.NewValue = CheckState.Unchecked

                ' Find and toggle all bookings for this date
                For i As Integer = 0 To lstBookingIds.Items.Count - 1
                    If i <> e.Index AndAlso TypeOf lstBookingIds.Items(i) Is BookingItem Then
                        Dim item As BookingItem = CType(lstBookingIds.Items(i), BookingItem)
                        ' Only check actual bookings (not separators) that match the date
                        If item.bookingID > 0 AndAlso item.BookingDate.ToString("yyyy-MM-dd") = dateStr Then
                            lstBookingIds.SetItemChecked(i, shouldCheck)
                        End If
                    End If
                Next

                ' Update the calculation
                BeginInvoke(New Action(AddressOf CalculateIncomeAmount))
                Return
            End If

            ' Prevent checking separator lines (bookingID = -1)
            If bookingItem.bookingID = -1 Then
                e.NewValue = CheckState.Unchecked
                Return
            End If
        End If

        ' For regular booking items, just update calculation
        BeginInvoke(New Action(AddressOf CalculateIncomeAmount))
    End Sub

    Private Sub lstExpenseBookingIds_ItemCheck(sender As Object, e As ItemCheckEventArgs)
        ' Use BeginInvoke to ensure the check state is updated before calculating
        BeginInvoke(New Action(AddressOf CalculateExpenseAmount))
    End Sub

    Private Sub CalculateIncomeAmount()
        Try
            Dim grossAmount As Decimal = 0
            Dim commissionRate As Decimal = 0.2 ' 20% commission

            ' Calculate total amount from selected bookings
            For Each row As DataGridViewRow In dgvBookings.Rows
                If Not row.IsNewRow AndAlso row.Cells("Selected").Value = True Then
                    Dim amountStr As String = row.Cells("Amount").Value.ToString()
                    Dim amount As Decimal = FinancialHelpers.CleanCurrencyString(amountStr)
                    grossAmount += amount
                End If
            Next

            ' Calculate commission and net amounts
            Dim commissionAmount As Decimal = grossAmount * commissionRate
            Dim netAmount As Decimal = grossAmount - commissionAmount

            ' Update labels to reflect the amounts
            lblComputedAmount.Text = "Gross: " & FinancialHelpers.FormatAsCurrency(grossAmount)
            lblComputedCommission.Text = "Commission (20%): " & FinancialHelpers.FormatAsCurrency(commissionAmount)
            lblComputedNet.Text = "Net (80%): " & FinancialHelpers.FormatAsCurrency(netAmount)

        Catch ex As Exception
            MessageBox.Show("Error calculating income amount: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub CalculateExpenseAmount()
        Try
            Dim totalAmount As Decimal = 0

            For i As Integer = 0 To lstExpenseBookingIds.CheckedItems.Count - 1
                If TypeOf lstExpenseBookingIds.CheckedItems(i) Is ExpenseItem Then
                    Dim expenseItem As ExpenseItem = CType(lstExpenseBookingIds.CheckedItems(i), ExpenseItem)
                    totalAmount += expenseItem.EstimatedAmount
                End If
            Next

            lblComputedExpenseAmount.Text = "₱" & totalAmount.ToString("N2")

        Catch ex As Exception
            MessageBox.Show("Error calculating expense amount: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnAddIncome_Click(sender As Object, e As EventArgs)
        Try
            Dim selectedRows As New List(Of DataGridViewRow)

            ' Get all selected rows
            For Each row As DataGridViewRow In dgvBookings.Rows
                If Not row.IsNewRow AndAlso row.Cells("Selected").Value = True Then
                    selectedRows.Add(row)
                End If
            Next

            If selectedRows.Count = 0 Then
                MessageBox.Show("Please select at least one booking to add as income.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Using conn As New MySqlConnection(connectionString)
                conn.Open()

                ' Get the default income source ID for tour bookings
                Dim incomeSourceID As Integer = GetDefaultIncomeSourceID("Tour Bookings")

                For Each row As DataGridViewRow In selectedRows
                    Dim bookingId As Integer = Convert.ToInt32(row.Cells("BookingID").Value)
                    Dim title As String = row.Cells("Title").Value.ToString()
                    Dim bookingDate As DateTime = Convert.ToDateTime(row.Cells("Date").Value)

                    ' Get the final amount from BookingAmounts view
                    Dim finalAmount As Decimal = 0
                    Dim queryGetAmount As String = "SELECT FinalAmount FROM BookingAmounts WHERE BookingID = @BookingID"
                    Using cmdGetAmount As New MySqlCommand(queryGetAmount, conn)
                        cmdGetAmount.Parameters.AddWithValue("@BookingID", bookingId)
                        finalAmount = Convert.ToDecimal(cmdGetAmount.ExecuteScalar())
                    End Using

                    ' Calculate amounts (20% commission)
                    Dim commissionAmount As Decimal = finalAmount * 0.2D
                    Dim netAmount As Decimal = finalAmount - commissionAmount

                    ' Insert into Income table
                    Dim queryInsert As String = "INSERT INTO Income (IncomeDate, Description, GrossAmount, CommissionAmount, NetAmount, " &
                "BookingID, IncomeItem, IncomeType, IncomeSourceID) " &
                "VALUES (@Date, @Description, @GrossAmount, @CommissionAmount, @NetAmount, " &
                "@BookingID, @IncomeItem, @IncomeType, @IncomeSourceID)"

                    Using cmdInsert As New MySqlCommand(queryInsert, conn)
                        cmdInsert.Parameters.AddWithValue("@Date", bookingDate)
                        cmdInsert.Parameters.AddWithValue("@Description", "Income from booking: " & title)
                        cmdInsert.Parameters.AddWithValue("@GrossAmount", finalAmount)
                        cmdInsert.Parameters.AddWithValue("@CommissionAmount", commissionAmount)
                        cmdInsert.Parameters.AddWithValue("@NetAmount", netAmount)
                        cmdInsert.Parameters.AddWithValue("@BookingID", bookingId)
                        cmdInsert.Parameters.AddWithValue("@IncomeItem", "Booking Commission")
                        cmdInsert.Parameters.AddWithValue("@IncomeType", "Operating")
                        cmdInsert.Parameters.AddWithValue("@IncomeSourceID", incomeSourceID)

                        cmdInsert.ExecuteNonQuery()
                    End Using
                Next

                MessageBox.Show($"{selectedRows.Count} booking(s) added as income successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

                ' Refresh data
                LoadBookings()
                LoadIncomeData()
                LoadDashboardData()

                conn.Close()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error adding income: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    ' Add this helper method to your form class
    Private Function GetDefaultIncomeSourceID(sourceName As String) As Integer
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim query As String = "SELECT IncomeSourceID FROM IncomeSources WHERE Category = @Category LIMIT 1"
                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@Category", sourceName)
                    Dim result = cmd.ExecuteScalar()
                    If result IsNot Nothing Then
                        Return Convert.ToInt32(result)
                    End If
                End Using
                conn.Close()
            End Using

            ' Fallback value if no matching record found
            Return 1
        Catch ex As Exception
            MessageBox.Show("Error getting income source: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return 1 ' Default fallback value
        End Try
    End Function
    Private Sub btnAddExpense_Click(sender As Object, e As EventArgs) Handles btnAddExpense.Click
        Try
            If ValidateExpenseInput() Then
                Using conn As New MySqlConnection(connectionString)
                    conn.Open()

                    ' Get selected expense items
                    Dim expenseItems As New List(Of String)
                    Dim totalAmount As Decimal = 0
                    Dim expenseType As String = "Operating" ' Default

                    For i As Integer = 0 To lstExpenseBookingIds.CheckedItems.Count - 1
                        If TypeOf lstExpenseBookingIds.CheckedItems(i) Is ExpenseItem Then
                            Dim expenseItem As ExpenseItem = CType(lstExpenseBookingIds.CheckedItems(i), ExpenseItem)
                            expenseItems.Add(expenseItem.ItemName)
                            totalAmount += expenseItem.EstimatedAmount
                            expenseType = expenseItem.CategoryType ' Use the category type from the item
                        End If
                    Next

                    Dim expenseItemsString As String = String.Join(", ", expenseItems)

                    Dim query As String = "INSERT INTO Expenses (ExpenseItem, ExpenseType, ExpenseCategoryID, Amount, " &
                                    "ExpenseDate, PaymentMethod, ReceiptNumber, Description) " &
                                    "VALUES (@ExpenseItem, @ExpenseType, @CategoryID, @Amount, @Date, @PaymentMethod, @ReceiptNumber, @Description)"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@ExpenseItem", expenseItemsString)
                        cmd.Parameters.AddWithValue("@ExpenseType", expenseType)
                        cmd.Parameters.AddWithValue("@CategoryID", CType(cmbExpenseCategory.SelectedItem, Object).Value)
                        cmd.Parameters.AddWithValue("@Amount", totalAmount)
                        cmd.Parameters.AddWithValue("@Date", dtpExpenseDate.Value)
                        cmd.Parameters.AddWithValue("@PaymentMethod", "Cash") ' Hardcoded as Cash
                        cmd.Parameters.AddWithValue("@ReceiptNumber", If(String.IsNullOrWhiteSpace(txtReceiptNumber.Text), DBNull.Value, txtReceiptNumber.Text))
                        cmd.Parameters.AddWithValue("@Description", txtExpenseDescription.Text)

                        cmd.ExecuteNonQuery()
                        MessageBox.Show("Expense record added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

                        ClearExpenseForm()
                        LoadExpenseData()
                        LoadDashboardData()
                    End Using
                    conn.Close()
                End Using
            End If
        Catch ex As Exception
            MessageBox.Show("Error adding expense: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnEditIncome_Click(sender As Object, e As EventArgs) Handles btnEditIncome.Click
        Try
            If dgvIncome.SelectedRows.Count > 0 Then
                Dim incomeId As Integer = Convert.ToInt32(dgvIncome.SelectedRows(0).Cells("IncomeID").Value)
                LoadIncomeForEdit(incomeId)
            Else
                MessageBox.Show("Please select an income record to edit.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            MessageBox.Show("Error editing income: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnEditExpense_Click(sender As Object, e As EventArgs) Handles btnEditExpense.Click
        Try
            If dgvExpenses.SelectedRows.Count > 0 Then
                Dim expenseId As Integer = Convert.ToInt32(dgvExpenses.SelectedRows(0).Cells("ExpenseID").Value)
                LoadExpenseForEdit(expenseId)
            Else
                MessageBox.Show("Please select an expense record to edit.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            MessageBox.Show("Error editing expense: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnDeleteIncome_Click(sender As Object, e As EventArgs) Handles btnDeleteIncome.Click
        Try
            If dgvIncome.SelectedRows.Count > 0 Then
                Dim result As DialogResult = MessageBox.Show("Are you sure you want to delete this income record?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If result = DialogResult.Yes Then
                    Dim incomeId As Integer = Convert.ToInt32(dgvIncome.SelectedRows(0).Cells("IncomeID").Value)

                    Using conn As New MySqlConnection(connectionString)
                        conn.Open()
                        Dim query As String = "DELETE FROM Income WHERE IncomeID = @IncomeID"
                        Using cmd As New MySqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@IncomeID", incomeId)
                            cmd.ExecuteNonQuery()

                            MessageBox.Show("Income record deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                            LoadIncomeData()
                            LoadDashboardData()
                        End Using
                        conn.Close()
                    End Using
                End If
            Else
                MessageBox.Show("Please select an income record to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            MessageBox.Show("Error deleting income: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnDeleteExpense_Click(sender As Object, e As EventArgs) Handles btnDeleteExpense.Click
        Try
            If dgvExpenses.SelectedRows.Count > 0 Then
                Dim result As DialogResult = MessageBox.Show("Are you sure you want to delete this expense record?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If result = DialogResult.Yes Then
                    Dim expenseId As Integer = Convert.ToInt32(dgvExpenses.SelectedRows(0).Cells("ExpenseID").Value)

                    Using conn As New MySqlConnection(connectionString)
                        conn.Open()
                        Dim query As String = "DELETE FROM Expenses WHERE ExpenseID = @ExpenseID"
                        Using cmd As New MySqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@ExpenseID", expenseId)
                            cmd.ExecuteNonQuery()

                            MessageBox.Show("Expense record deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                            LoadExpenseData()
                            LoadDashboardData()
                        End Using
                        conn.Close()
                    End Using
                End If
            Else
                MessageBox.Show("Please select an expense record to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            MessageBox.Show("Error deleting expense: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnGenerateReport_Click(sender As Object, e As EventArgs) Handles btnGenerateReport.Click
        Try
            GenerateFinancialReport()
        Catch ex As Exception
            MessageBox.Show("Error generating report: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnExportReport_Click(sender As Object, e As EventArgs) Handles btnExportReport.Click
        Try
            ExportReportToExcel()
        Catch ex As Exception
            MessageBox.Show("Error exporting report: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function ValidateIncomeInput() As Boolean
        If cmbIncomeSource.SelectedIndex = -1 Then
            MessageBox.Show("Please select an income source.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        If lstBookingIds.CheckedItems.Count = 0 Then
            MessageBox.Show("Please select at least one booking.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        If String.IsNullOrWhiteSpace(txtIncomeDescription.Text) Then
            MessageBox.Show("Please enter a description.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        Return True
    End Function

    Private Function ValidateExpenseInput() As Boolean
        If cmbExpenseCategory.SelectedIndex = -1 Then
            MessageBox.Show("Please select an expense category.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        If lstExpenseBookingIds.CheckedItems.Count = 0 Then
            MessageBox.Show("Please select at least one expense item.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        If String.IsNullOrWhiteSpace(txtExpenseDescription.Text) Then
            MessageBox.Show("Please enter a description.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        Return True
    End Function
    Private Sub ClearIncomeForm()
        cmbIncomeSource.SelectedIndex = -1
        dtpIncomeDate.Value = DateTime.Now
        txtIncomeDescription.Clear()
        For i As Integer = 0 To lstBookingIds.Items.Count - 1
            lstBookingIds.SetItemChecked(i, False)
        Next
        lblComputedAmount.Text = "₱0.00"
        lblComputedCommission.Text = "Commission: ₱0.00"
        lblComputedNet.Text = "Net Amount: ₱0.00"
    End Sub

    Private Sub ClearExpenseForm()
        cmbExpenseCategory.SelectedIndex = -1
        dtpExpenseDate.Value = DateTime.Now
        txtExpenseDescription.Clear()
        txtReceiptNumber.Clear()
        For i As Integer = 0 To lstExpenseBookingIds.Items.Count - 1
            lstExpenseBookingIds.SetItemChecked(i, False)
        Next
        lblComputedExpenseAmount.Text = "₱0.00"
    End Sub

    Private Sub LoadIncomeForEdit(incomeId As Integer)
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim query As String = "SELECT * FROM Income WHERE IncomeID = @IncomeID"
                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@IncomeID", incomeId)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            ' Set form values
                            For i As Integer = 0 To cmbIncomeSource.Items.Count - 1
                                If cmbIncomeSource.Items(i).Value.ToString() = reader("IncomeSourceID").ToString() Then
                                    cmbIncomeSource.SelectedIndex = i
                                    Exit For
                                End If
                            Next

                            dtpIncomeDate.Value = Convert.ToDateTime(reader("IncomeDate"))
                            txtIncomeDescription.Text = reader("Description").ToString()

                            ' Calculate the original gross amount (which is stored as GrossAmount)
                            Dim storedGrossAmount As Decimal = Convert.ToDecimal(reader("GrossAmount"))
                            Dim storedCommission As Decimal = Convert.ToDecimal(reader("CommissionAmount"))
                            Dim storedNetAmount As Decimal = Convert.ToDecimal(reader("NetAmount"))

                            ' Check corresponding bookings
                            Dim bookingIds As String = reader("BookingID").ToString()
                            If Not String.IsNullOrEmpty(bookingIds) Then
                                Dim ids As String() = bookingIds.Split(","c)
                                For i As Integer = 0 To lstBookingIds.Items.Count - 1
                                    Dim booking As BookingItem = CType(lstBookingIds.Items(i), BookingItem)
                                    If ids.Contains(booking.bookingID.ToString()) Then
                                        lstBookingIds.SetItemChecked(i, True)
                                    End If
                                Next
                            End If

                            ' Update the displayed amounts
                            CalculateIncomeAmount()

                            ' Switch to income tab
                            tabControl.SelectedTab = incomeTab
                        End If
                    End Using
                End Using
                conn.Close()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading income for edit: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LoadExpenseForEdit(expenseId As Integer)
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim query As String = "SELECT e.*, ec.Category FROM Expenses e " &
                            "INNER JOIN ExpenseCategories ec ON e.ExpenseCategoryID = ec.ExpenseCategoryID " &
                            "WHERE e.ExpenseID = @ExpenseID"
                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@ExpenseID", expenseId)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            ' Set form values
                            For i As Integer = 0 To cmbExpenseCategory.Items.Count - 1
                                If CType(cmbExpenseCategory.Items(i), Object).Value.ToString() = reader("ExpenseCategoryID").ToString() Then
                                    cmbExpenseCategory.SelectedIndex = i
                                    Exit For
                                End If
                            Next

                            dtpExpenseDate.Value = Convert.ToDateTime(reader("ExpenseDate"))
                            txtExpenseDescription.Text = If(IsDBNull(reader("Description")), "", reader("Description").ToString())
                            txtReceiptNumber.Text = If(IsDBNull(reader("ReceiptNumber")), "", reader("ReceiptNumber").ToString())

                            ' Load expense items for the selected category first
                            LoadExpenseItemsByCategory()

                            ' Check corresponding expense items if ExpenseItem column exists
                            If Not IsDBNull(reader("ExpenseItem")) Then
                                Dim expenseItems As String = reader("ExpenseItem").ToString()
                                If Not String.IsNullOrEmpty(expenseItems) Then
                                    Dim items As String() = expenseItems.Split(New String() {", "}, StringSplitOptions.RemoveEmptyEntries)
                                    For i As Integer = 0 To lstExpenseBookingIds.Items.Count - 1
                                        If TypeOf lstExpenseBookingIds.Items(i) Is ExpenseItem Then
                                            Dim expenseItem As ExpenseItem = CType(lstExpenseBookingIds.Items(i), ExpenseItem)
                                            If items.Contains(expenseItem.ItemName) Then
                                                lstExpenseBookingIds.SetItemChecked(i, True)
                                            End If
                                        End If
                                    Next
                                End If
                            End If

                            ' Update computed amount
                            CalculateExpenseAmount()

                            ' Switch to expense tab
                            tabControl.SelectedTab = expenseTab
                        End If
                    End Using
                End Using
                conn.Close()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading expense for edit: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub GenerateFinancialReport()
        Try
            Dim startDate As DateTime = dtpReportStart.Value.Date
            Dim endDate As DateTime = dtpReportEnd.Value.Date.AddDays(1).AddSeconds(-1)

            Using conn As New MySqlConnection(connectionString)
                conn.Open()

                ' Main query that properly calculates commission (20% of FinalAmount)
                Dim query As String = "SELECT 
                DATE(TransactionDate) AS TransactionDate,
                SUM(OurIncome) AS OurIncome,
                SUM(TotalExpenses) AS TotalExpenses,
                SUM(OurIncome - TotalExpenses) AS NetProfit
                FROM (
                    -- Income with proper 20% commission calculation
                    SELECT 
                        i.IncomeDate AS TransactionDate,
                        (ba.FinalAmount * 0.2) AS OurIncome,
                        0 AS TotalExpenses
                    FROM Income i
                    JOIN Bookings b ON i.BookingID = b.BookingID
                    JOIN BookingAmounts ba ON b.BookingID = ba.BookingID
                    WHERE i.IncomeDate BETWEEN @StartDate AND @EndDate
                    
                    UNION ALL
                    
                    -- Expenses
                    SELECT 
                        e.ExpenseDate AS TransactionDate,
                        0 AS OurIncome,
                        e.Amount AS TotalExpenses
                    FROM Expenses e
                    WHERE e.ExpenseDate BETWEEN @StartDate AND @EndDate
                ) AS CombinedData
                GROUP BY DATE(TransactionDate)
                ORDER BY TransactionDate DESC"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@StartDate", startDate)
                    cmd.Parameters.AddWithValue("@EndDate", endDate)

                    Using adapter As New MySqlDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        ' Add formatted columns
                        dt.Columns.Add("FormattedIncome", GetType(String))
                        dt.Columns.Add("FormattedExpenses", GetType(String))
                        dt.Columns.Add("FormattedProfit", GetType(String))

                        For Each row As DataRow In dt.Rows
                            Dim income = If(row.IsNull("OurIncome"), 0D, Convert.ToDecimal(row("OurIncome")))
                            Dim expenses = If(row.IsNull("TotalExpenses"), 0D, Convert.ToDecimal(row("TotalExpenses")))
                            Dim profit = If(row.IsNull("NetProfit"), 0D, Convert.ToDecimal(row("NetProfit")))

                            row("FormattedIncome") = FinancialHelpers.FormatAsCurrency(income)
                            row("FormattedExpenses") = FinancialHelpers.FormatAsCurrency(expenses)
                            row("FormattedProfit") = FinancialHelpers.FormatAsCurrency(profit)
                            row("TransactionDate") = Convert.ToDateTime(row("TransactionDate")).ToString("yyyy-MM-dd")
                        Next

                        ' Bind to grid with null checks
                        If dgvReport IsNot Nothing Then
                            dgvReport.DataSource = dt

                            ' Configure column visibility
                            dgvReport.Columns("OurIncome").Visible = False
                            dgvReport.Columns("TotalExpenses").Visible = False
                            dgvReport.Columns("NetProfit").Visible = False

                            ' Set column headers
                            dgvReport.Columns("FormattedIncome").HeaderText = "Our Income (20%)"
                            dgvReport.Columns("FormattedExpenses").HeaderText = "Total Expenses"
                            dgvReport.Columns("FormattedProfit").HeaderText = "Net Profit"
                            dgvReport.Columns("TransactionDate").HeaderText = "Date"
                        End If
                    End Using
                End Using

                ' Calculate summary with proper commission
                CalculateReportSummary(startDate, endDate)

                conn.Close()
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error generating report: {ex.Message}", "Report Error",
                       MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub CalculateReportSummary(startDate As DateTime, endDate As DateTime)
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()

                ' Calculate total income (20% commission)
                Dim incomeQuery As String = "SELECT COALESCE(SUM(ba.FinalAmount * 0.2), 0) 
                                       FROM Income i
                                       JOIN Bookings b ON i.BookingID = b.BookingID
                                       JOIN BookingAmounts ba ON b.BookingID = ba.BookingID
                                       WHERE i.IncomeDate BETWEEN @StartDate AND @EndDate"

                Dim totalIncome As Decimal = 0
                Using cmd As New MySqlCommand(incomeQuery, conn)
                    cmd.Parameters.AddWithValue("@StartDate", startDate)
                    cmd.Parameters.AddWithValue("@EndDate", endDate)
                    totalIncome = Convert.ToDecimal(cmd.ExecuteScalar())
                End Using

                ' Calculate total expenses
                Dim expenseQuery As String = "SELECT COALESCE(SUM(Amount), 0) 
                                        FROM Expenses 
                                        WHERE ExpenseDate BETWEEN @StartDate AND @EndDate"

                Dim totalExpenses As Decimal = 0
                Using cmd As New MySqlCommand(expenseQuery, conn)
                    cmd.Parameters.AddWithValue("@StartDate", startDate)
                    cmd.Parameters.AddWithValue("@EndDate", endDate)
                    totalExpenses = Convert.ToDecimal(cmd.ExecuteScalar())
                End Using

                ' Update UI with null checks
                If lblReportIncome IsNot Nothing Then
                    lblReportIncome.Text = FinancialHelpers.FormatAsCurrency(totalIncome)
                End If
                If lblReportExpenses IsNot Nothing Then
                    lblReportExpenses.Text = FinancialHelpers.FormatAsCurrency(totalExpenses)
                End If

                Dim netProfit As Decimal = totalIncome - totalExpenses
                If lblReportProfit IsNot Nothing Then
                    lblReportProfit.Text = FinancialHelpers.FormatAsCurrency(netProfit)
                End If

                Dim profitMargin As Decimal = If(totalIncome > 0, (netProfit / totalIncome) * 100, 0)
                If lblReportMargin IsNot Nothing Then
                    lblReportMargin.Text = $"{profitMargin:N1}%"
                End If

                conn.Close()
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error calculating summary: {ex.Message}", "Calculation Error",
                       MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub ExportReportToExcel()
        Try
            If dgvReport.Rows.Count = 0 Then
                MessageBox.Show("No data to export. Please generate a report first.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim saveDialog As New SaveFileDialog()
            saveDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*"
            saveDialog.FilterIndex = 1
            saveDialog.RestoreDirectory = True
            saveDialog.FileName = "Financial_Report_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".xlsx"

            If saveDialog.ShowDialog() = DialogResult.OK Then
                ' Create Excel application
                Dim excelApp As Object = CreateObject("Excel.Application")
                Dim workbook As Object = excelApp.Workbooks.Add()
                Dim worksheet As Object = workbook.Worksheets(1)

                ' Set worksheet name
                worksheet.Name = "Financial Report"

                ' Add headers
                For col As Integer = 0 To dgvReport.Columns.Count - 1
                    worksheet.Cells(1, col + 1) = dgvReport.Columns(col).HeaderText
                Next

                ' Add data
                For row As Integer = 0 To dgvReport.Rows.Count - 1
                    For col As Integer = 0 To dgvReport.Columns.Count - 1
                        worksheet.Cells(row + 2, col + 1) = dgvReport.Rows(row).Cells(col).Value.ToString()
                    Next
                Next

                ' Add summary section
                Dim summaryStartRow As Integer = dgvReport.Rows.Count + 4
                worksheet.Cells(summaryStartRow, 1) = "REPORT SUMMARY"
                worksheet.Cells(summaryStartRow + 1, 1) = "Period:"
                worksheet.Cells(summaryStartRow + 1, 2) = dtpReportStart.Value.ToString("yyyy-MM-dd") & " to " & dtpReportEnd.Value.ToString("yyyy-MM-dd")
                worksheet.Cells(summaryStartRow + 2, 1) = "Total Income:"
                worksheet.Cells(summaryStartRow + 2, 2) = lblReportIncome.Text
                worksheet.Cells(summaryStartRow + 3, 1) = "Total Expenses:"
                worksheet.Cells(summaryStartRow + 3, 2) = lblReportExpenses.Text
                worksheet.Cells(summaryStartRow + 4, 1) = "Net Profit:"
                worksheet.Cells(summaryStartRow + 4, 2) = lblReportProfit.Text
                worksheet.Cells(summaryStartRow + 5, 1) = "Profit Margin:"
                worksheet.Cells(summaryStartRow + 5, 2) = lblReportMargin.Text

                ' Format headers
                Dim headerRange As Object = worksheet.Range("A1:" & Chr(64 + dgvReport.Columns.Count) & "1")
                headerRange.Font.Bold = True
                headerRange.Interior.Color = RGB(200, 200, 200)

                ' Format summary headers
                Dim summaryRange As Object = worksheet.Range("A" & summaryStartRow & ":A" & (summaryStartRow + 5))
                summaryRange.Font.Bold = True

                ' Auto-fit columns
                worksheet.Columns.AutoFit()

                ' Save the file
                workbook.SaveAs(saveDialog.FileName)
                workbook.Close()
                excelApp.Quit()

                ' Release COM objects
                System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet)
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook)
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp)

                MessageBox.Show("Report exported successfully to: " & saveDialog.FileName, "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

        Catch ex As Exception
            MessageBox.Show("Error exporting report: " & ex.Message & vbCrLf & vbCrLf & "Make sure Microsoft Excel is installed on this system.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Form_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            ' Perform any cleanup operations here
            ' Save user preferences, close connections, etc.
        Catch ex As Exception
            MessageBox.Show("Error during form closing: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub RefreshAllData()
        Try
            LoadIncomeSources()
            LoadExpenseCategories()
            LoadBookings()
            LoadIncomeData()
            LoadExpenseData()
            LoadDashboardData()
        Catch ex As Exception
            MessageBox.Show("Error refreshing data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    Private Sub dgvIncome_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvIncome.CellDoubleClick
        If e.RowIndex >= 0 Then
            btnEditIncome_Click(sender, e)
        End If
    End Sub

    Private Sub dgvExpenses_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvExpenses.CellDoubleClick
        If e.RowIndex >= 0 Then
            btnEditExpense_Click(sender, e)
        End If
    End Sub

    ' Additional helper methods for enhanced functionality
    Private Sub SearchIncomeData(searchTerm As String)
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim query As String = "SELECT i.IncomeID, i.IncomeDate, iss.Category as Source, i.Description, " &
                                   "i.GrossAmount, i.CommissionAmount, i.NetAmount, i.BookingID " &
                                   "FROM Income i " &
                                   "INNER JOIN IncomeSources iss ON i.IncomeSourceID = iss.IncomeSourceID " &
                                   "WHERE i.Description LIKE @SearchTerm OR iss.Category LIKE @SearchTerm " &
                                   "ORDER BY i.IncomeDate DESC"

                Using adapter As New MySqlDataAdapter(query, conn)
                    adapter.SelectCommand.Parameters.AddWithValue("@SearchTerm", "%" & searchTerm & "%")
                    Dim dt As New DataTable()
                    adapter.Fill(dt)

                    ' Format the data for display
                    For Each row As DataRow In dt.Rows
                        row("GrossAmount") = "₱" & Convert.ToDecimal(row("GrossAmount")).ToString("N2")
                        row("CommissionAmount") = "₱" & Convert.ToDecimal(row("CommissionAmount")).ToString("N2")
                        row("NetAmount") = "₱" & Convert.ToDecimal(row("NetAmount")).ToString("N2")
                        row("IncomeDate") = Convert.ToDateTime(row("IncomeDate")).ToString("yyyy-MM-dd")
                    Next

                    dgvIncome.DataSource = dt
                End Using
                conn.Close()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error searching income data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub SearchExpenseData(searchTerm As String)
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim query As String = "SELECT e.ExpenseID, e.ExpenseDate, ec.Category, e.Description, " &
                                   "e.Amount, e.PaymentMethod, e.ReceiptNumber, e.BookingID " &
                                   "FROM Expenses e " &
                                   "INNER JOIN ExpenseCategories ec ON e.ExpenseCategoryID = ec.ExpenseCategoryID " &
                                   "WHERE e.Description LIKE @SearchTerm OR ec.Category LIKE @SearchTerm " &
                                   "ORDER BY e.ExpenseDate DESC"

                Using adapter As New MySqlDataAdapter(query, conn)
                    adapter.SelectCommand.Parameters.AddWithValue("@SearchTerm", "%" & searchTerm & "%")
                    Dim dt As New DataTable()
                    adapter.Fill(dt)

                    ' Format the data for display
                    For Each row As DataRow In dt.Rows
                        row("Amount") = "₱" & Convert.ToDecimal(row("Amount")).ToString("N2")
                        row("ExpenseDate") = Convert.ToDateTime(row("ExpenseDate")).ToString("yyyy-MM-dd")
                    Next

                    dgvExpenses.DataSource = dt
                End Using
                conn.Close()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error searching expense data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ValidateBusinessRules()
        ' Add any business rule validations here
        ' For example: checking for duplicate entries, date validations, etc.
    End Sub

End Class

' Helper class for booking items
Public Class BookingItems
    Public Property BookingID As Integer
    Public Property BookingReference As String
    Public Property Amount As Decimal
    Public Property ClientName As String
    Public Property BookingDate As DateTime
    Public Property DisplayText As String
    Public Property Title As String

    Public Class DateSeparator
        Public Property DisplayText As String
    End Class

    Public Overrides Function ToString() As String
        Return $"{BookingReference} - {ClientName} (₱{Amount:N2})"
    End Function
End Class

Public Class ExpenseItem
    Public Property ItemName As String
    Public Property EstimatedAmount As Decimal
    Public Property CategoryType As String
    Public Property Description As String

    Public ReadOnly Property DisplayText As String
        Get
            Return $"{ItemName} - ₱{EstimatedAmount:N2}"
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return DisplayText
    End Function
End Class