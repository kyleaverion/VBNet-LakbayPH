Imports System.IO
Imports System.Linq
Imports System.Text
Imports MySql.Data.MySqlClient
Imports projectv2.BookingForm

Public Class BookingListForm
    Inherits Form

    Dim connectionString As String = "Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;"
    Private WithEvents tabControl As New TabControl()

    ' Control buttons
    Private WithEvents btnSelectAll As New Button()
    Private WithEvents btnClearSelection As New Button()
    Private WithEvents btnDeleteSelected As New Button()
    Private WithEvents btnEditSelected As New Button()
    Private WithEvents btnSubmitSelected As New Button()
    Private WithEvents lblSelectedCount As New Label()

    ' User info for booking submission
    Public Property CurrentUserID As Integer
    Public Property CurrentUserName As String = ""
    Public Property CurrentUserEmail As String = ""
    Public Property CurrentUserPhone As String = ""

    ' Promo discount properties
    Private availablePromos As New List(Of PromoDiscounts)()
    Private selectedPromoID As Integer = 0
    Private appliedPromoDetails As PromoDiscounts = Nothing
    Private allPromoTypes As New List(Of PromoDiscounts)()

    ' Promo Discount class
    Public Class PromoDiscounts
        Public Property PromoID As Integer
        Public Property PromoTypeID As Integer
        Public Property PromoTypeName As String
        Public Property DiscountPercentage As Decimal
        Public Property DocumentNumber As String
        Public Property UserID As Integer
        Public Property Status As String
        Public Property ExpiryDate As DateTime?
    End Class

    Private availableAddOns As New List(Of PackageAddOns)()
    Private selectedBookings As Object

    Public Class PackageAddOns
        Public Property AddOnID As Integer
        Public Property packageID As Integer
        Public Property AddOnName As String
        Public Property Price As Decimal
        Public Property Unit As String
    End Class

    Public Sub New()
        InitializeComponents()
        LoadCurrentUserInfo()
    End Sub

    Public Sub New(userID As Integer, userName As String, userEmail As String, userPhone As String)
        CurrentUserID = userID
        CurrentUserName = userName
        CurrentUserEmail = userEmail
        CurrentUserPhone = userPhone
        InitializeComponents()
        LoadUserPromoDiscounts()
    End Sub

    Private Sub InitializeComponents()
        Me.Text = "Booking List Management"
        Me.Size = New Size(1200, 800)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.White
        Me.MinimumSize = New Size(1200, 800)

        ' Header
        Dim lblTitle As New Label()
        lblTitle.Text = "Manage Your Booking List"
        lblTitle.Font = New Font("Segoe UI", 16, FontStyle.Bold)
        lblTitle.ForeColor = Color.FromArgb(25, 118, 210)
        lblTitle.Location = New Point(20, 15)
        lblTitle.Size = New Size(400, 30)
        Me.Controls.Add(lblTitle)

        ' Selection info label
        lblSelectedCount.Text = "Selected: 0 bookings"
        lblSelectedCount.Font = New Font("Segoe UI", 10)
        lblSelectedCount.ForeColor = Color.FromArgb(100, 100, 100)
        lblSelectedCount.Location = New Point(20, 50)
        lblSelectedCount.Size = New Size(200, 20)
        Me.Controls.Add(lblSelectedCount)

        ' Control buttons panel
        Dim buttonPanel As New Panel()
        buttonPanel.Location = New Point(20, 75)
        buttonPanel.Size = New Size(1150, 50)
        buttonPanel.BackColor = Color.FromArgb(248, 249, 250)
        buttonPanel.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(buttonPanel)

        ' Select All button
        btnSelectAll.Text = "Select All"
        btnSelectAll.Location = New Point(10, 10)
        btnSelectAll.Size = New Size(90, 30)
        btnSelectAll.BackColor = Color.FromArgb(33, 150, 243)
        btnSelectAll.ForeColor = Color.White
        btnSelectAll.FlatStyle = FlatStyle.Flat
        btnSelectAll.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        buttonPanel.Controls.Add(btnSelectAll)

        ' Clear Selection button
        btnClearSelection.Text = "Clear Selection"
        btnClearSelection.Location = New Point(110, 10)
        btnClearSelection.Size = New Size(100, 30)
        btnClearSelection.BackColor = Color.FromArgb(156, 39, 176)
        btnClearSelection.ForeColor = Color.White
        btnClearSelection.FlatStyle = FlatStyle.Flat
        btnClearSelection.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        buttonPanel.Controls.Add(btnClearSelection)

        ' Delete Selected button
        btnDeleteSelected.Text = "Delete Selected"
        btnDeleteSelected.Location = New Point(220, 10)
        btnDeleteSelected.Size = New Size(110, 30)
        btnDeleteSelected.BackColor = Color.FromArgb(244, 67, 54)
        btnDeleteSelected.ForeColor = Color.White
        btnDeleteSelected.FlatStyle = FlatStyle.Flat
        btnDeleteSelected.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        buttonPanel.Controls.Add(btnDeleteSelected)

        ' Edit Selected button
        btnEditSelected.Text = "Edit Selected"
        btnEditSelected.Location = New Point(340, 10)
        btnEditSelected.Size = New Size(100, 30)
        btnEditSelected.BackColor = Color.FromArgb(255, 152, 0)
        btnEditSelected.ForeColor = Color.White
        btnEditSelected.FlatStyle = FlatStyle.Flat
        btnEditSelected.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        buttonPanel.Controls.Add(btnEditSelected)

        ' Submit Selected button
        btnSubmitSelected.Text = "Submit Selected"
        btnSubmitSelected.Location = New Point(450, 10)
        btnSubmitSelected.Size = New Size(120, 30)
        btnSubmitSelected.BackColor = Color.FromArgb(76, 175, 80)
        btnSubmitSelected.ForeColor = Color.White
        btnSubmitSelected.FlatStyle = FlatStyle.Flat
        btnSubmitSelected.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        buttonPanel.Controls.Add(btnSubmitSelected)

        ' Tab Control
        tabControl.Location = New Point(20, 135)
        tabControl.Size = New Size(1150, 620)
        Me.Controls.Add(tabControl)

        ' Create tabs for different package types
        CreatePackageTypeTabs()
    End Sub

    Private Sub CreatePackageTypeTabs()
        tabControl.TabPages.Clear()

        ' Get unique package types
        Dim packageTypes = BookingForm.BookingList.Select(Function(b) b.PackageType).Distinct().ToList()

        If packageTypes.Count = 0 Then
            Dim emptyTab As New TabPage("No Bookings")
            Dim lblEmpty As New Label()
            lblEmpty.Text = "No bookings added yet."
            lblEmpty.Location = New Point(50, 50)
            lblEmpty.Size = New Size(200, 30)
            lblEmpty.Font = New Font("Segoe UI", 12)
            lblEmpty.ForeColor = Color.Gray
            emptyTab.Controls.Add(lblEmpty)
            tabControl.TabPages.Add(emptyTab)
            Return
        End If

        For Each packageType In packageTypes
            Dim tabPage As New TabPage(packageType)
            Dim dgv As New DataGridView()

            ' Configure DataGridView
            dgv.Location = New Point(10, 10)
            dgv.Size = New Size(1120, 580)
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            dgv.ReadOnly = False
            dgv.AllowUserToAddRows = False
            dgv.AllowUserToDeleteRows = False
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            dgv.MultiSelect = True
            dgv.BackgroundColor = Color.White
            dgv.BorderStyle = BorderStyle.Fixed3D
            dgv.Font = New Font("Segoe UI", 9)
            dgv.Name = $"dgv_{packageType}"

            ' Add checkbox column for selection
            Dim chkColumn As New DataGridViewCheckBoxColumn()
            chkColumn.Name = "Select"
            chkColumn.HeaderText = "Select"
            chkColumn.Width = 60
            chkColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            chkColumn.ReadOnly = False
            chkColumn.Resizable = DataGridViewTriState.False
            dgv.Columns.Add(chkColumn)

            ' Add data columns with fixed widths
            dgv.Columns.Add("PackageName", "Package Name")
            dgv.Columns.Add("NumberOfPeople", "People")
            dgv.Columns.Add("AddOns", "Add-ons")
            dgv.Columns.Add("TotalPrice", "Total Price")
            dgv.Columns.Add("BookingDate", "Date")
            dgv.Columns.Add("UniqueID", "UniqueID")

            ' Set specific column widths instead of FillWeight
            dgv.Columns("PackageName").Width = 250
            dgv.Columns("NumberOfPeople").Width = 80
            dgv.Columns("AddOns").Width = 300
            dgv.Columns("TotalPrice").Width = 120
            dgv.Columns("BookingDate").Width = 120
            dgv.Columns("UniqueID").Visible = False

            ' Set remaining columns to not auto-resize
            For Each col As DataGridViewColumn In dgv.Columns
                If col.Name <> "Select" Then
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                    col.Resizable = DataGridViewTriState.True
                End If
            Next

            ' Style the header
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(63, 81, 181)
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            dgv.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
            dgv.ColumnHeadersHeight = 35

            ' Load data for this package type
            Dim typeBookings = BookingForm.BookingList.Where(Function(b) b.PackageType = packageType).ToList()
            For Each booking In typeBookings
                dgv.Rows.Add(False, booking.PackageName, booking.NumberOfPeople,
                           booking.AddOns, $"₱{booking.TotalPrice:N2}",
                           booking.BookingDate.ToString("MMM dd, yyyy"), booking.UniqueID)
            Next

            ' Add event handler for checkbox changes
            AddHandler dgv.CellValueChanged, AddressOf dgv_CellValueChanged
            AddHandler dgv.CurrentCellDirtyStateChanged, AddressOf dgv_CurrentCellDirtyStateChanged

            tabPage.Controls.Add(dgv)
            tabControl.TabPages.Add(tabPage)
        Next

        UpdateSelectedCount()
    End Sub

    Private Sub dgv_CurrentCellDirtyStateChanged(sender As Object, e As EventArgs)
        Dim dgv As DataGridView = CType(sender, DataGridView)
        If dgv.IsCurrentCellDirty Then
            dgv.CommitEdit(DataGridViewDataErrorContexts.Commit)
        End If
    End Sub

    Private Sub dgv_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs)
        If e.ColumnIndex = 0 AndAlso e.RowIndex >= 0 Then ' Checkbox column
            UpdateSelectedCount()
        End If
    End Sub

    Private Sub UpdateSelectedCount()
        Dim selectedCount As Integer = 0

        For Each tabPage As TabPage In tabControl.TabPages
            For Each control As Control In tabPage.Controls
                If TypeOf control Is DataGridView Then
                    Dim dgv As DataGridView = CType(control, DataGridView)
                    For Each row As DataGridViewRow In dgv.Rows
                        If CBool(row.Cells("Select").Value) Then
                            selectedCount += 1
                        End If
                    Next
                End If
            Next
        Next

        lblSelectedCount.Text = $"Selected: {selectedCount} booking(s)"

        ' Enable/disable buttons based on selection
        btnDeleteSelected.Enabled = selectedCount > 0
        btnEditSelected.Enabled = selectedCount = 1 ' Only allow editing one at a time
        btnSubmitSelected.Enabled = selectedCount > 0
        btnClearSelection.Enabled = selectedCount > 0
    End Sub

    Private Sub btnSelectAll_Click(sender As Object, e As EventArgs) Handles btnSelectAll.Click
        For Each tabPage As TabPage In tabControl.TabPages
            For Each control As Control In tabPage.Controls
                If TypeOf control Is DataGridView Then
                    Dim dgv As DataGridView = CType(control, DataGridView)
                    For Each row As DataGridViewRow In dgv.Rows
                        row.Cells("Select").Value = True
                    Next
                End If
            Next
        Next
        UpdateSelectedCount()
    End Sub

    Private Sub btnClearSelection_Click(sender As Object, e As EventArgs) Handles btnClearSelection.Click
        For Each tabPage As TabPage In tabControl.TabPages
            For Each control As Control In tabPage.Controls
                If TypeOf control Is DataGridView Then
                    Dim dgv As DataGridView = CType(control, DataGridView)
                    For Each row As DataGridViewRow In dgv.Rows
                        row.Cells("Select").Value = False
                    Next
                End If
            Next
        Next
        UpdateSelectedCount()
    End Sub

    Private Sub btnDeleteSelected_Click(sender As Object, e As EventArgs) Handles btnDeleteSelected.Click
        Dim selectedBookings As New List(Of String)()

        ' Collect selected booking unique IDs
        For Each tabPage As TabPage In tabControl.TabPages
            For Each control As Control In tabPage.Controls
                If TypeOf control Is DataGridView Then
                    Dim dgv As DataGridView = CType(control, DataGridView)
                    For Each row As DataGridViewRow In dgv.Rows
                        If CBool(row.Cells("Select").Value) Then
                            selectedBookings.Add(row.Cells("UniqueID").Value.ToString())
                        End If
                    Next
                End If
            Next
        Next

        If selectedBookings.Count = 0 Then
            MessageBox.Show("No bookings selected for deletion.", "No Selection",
                          MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim result As DialogResult = MessageBox.Show(
            $"Are you sure you want to delete {selectedBookings.Count} selected booking(s)?",
            "Confirm Deletion",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question)

        If result = DialogResult.Yes Then
            ' Remove selected bookings from the list
            For Each uniqueID In selectedBookings
                BookingForm.BookingList.RemoveAll(Function(b) b.UniqueID = uniqueID)
            Next

            ' Refresh the display
            CreatePackageTypeTabs()

            MessageBox.Show($"{selectedBookings.Count} booking(s) deleted successfully.",
                          "Deletion Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub btnEditSelected_Click(sender As Object, e As EventArgs) Handles btnEditSelected.Click
        Dim selectedBooking As BookingForm.BookingItem = Nothing

        ' Find the single selected booking
        For Each tabPage As TabPage In tabControl.TabPages
            For Each control As Control In tabPage.Controls
                If TypeOf control Is DataGridView Then
                    Dim dgv As DataGridView = CType(control, DataGridView)
                    For Each row As DataGridViewRow In dgv.Rows
                        If CBool(row.Cells("Select").Value) Then
                            Dim uniqueID = row.Cells("UniqueID").Value.ToString()
                            selectedBooking = BookingForm.BookingList.FirstOrDefault(Function(b) b.UniqueID = uniqueID)
                            Exit For
                        End If
                    Next
                End If
            Next
        Next

        If selectedBooking IsNot Nothing Then
            ' Open edit form
            ShowEditBookingDialog(selectedBooking)
        Else
            MessageBox.Show("Please select exactly one booking to edit.", "Selection Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    Private Sub ShowEditBookingDialog(booking As BookingForm.BookingItem)
        ' Create a simple edit dialog
        Dim editForm As New Form()
        editForm.Text = "Edit Booking"
        editForm.Size = New Size(500, 600)
        editForm.StartPosition = FormStartPosition.CenterParent
        editForm.FormBorderStyle = FormBorderStyle.FixedDialog
        editForm.MaximizeBox = False
        editForm.MinimizeBox = False

        Dim yPos As Integer = 20

        ' Number of People
        Dim lblPeople As New Label()
        lblPeople.Text = "Number of People:"
        lblPeople.Location = New Point(20, yPos)
        lblPeople.Size = New Size(120, 20)
        editForm.Controls.Add(lblPeople)

        Dim nudPeople As New NumericUpDown()
        nudPeople.Location = New Point(150, yPos)
        nudPeople.Size = New Size(100, 25)
        nudPeople.Minimum = 1
        nudPeople.Maximum = 50
        nudPeople.Value = booking.NumberOfPeople
        editForm.Controls.Add(nudPeople)

        yPos += 40

        ' Booking Date
        Dim lblDate As New Label()
        lblDate.Text = "Booking Date:"
        lblDate.Location = New Point(20, yPos)
        lblDate.Size = New Size(120, 20)
        editForm.Controls.Add(lblDate)

        Dim dtpDate As New DateTimePicker()
        dtpDate.Location = New Point(150, yPos)
        dtpDate.Size = New Size(200, 25)
        dtpDate.Value = booking.BookingDate
        dtpDate.MinDate = DateTime.Today
        editForm.Controls.Add(dtpDate)

        yPos += 40

        ' Add-ons Section
        Dim lblAddOns As New Label()
        lblAddOns.Text = "Available Add-ons:"
        lblAddOns.Location = New Point(20, yPos)
        lblAddOns.Size = New Size(150, 20)
        lblAddOns.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        editForm.Controls.Add(lblAddOns)

        yPos += 30

        ' Load add-ons for this package
        LoadAddOnsForPackage(booking.PackageID)

        ' Create scrollable panel for add-ons
        Dim addOnPanel As New Panel()
        addOnPanel.Location = New Point(20, yPos)
        addOnPanel.Size = New Size(440, 200)
        addOnPanel.BorderStyle = BorderStyle.FixedSingle
        addOnPanel.AutoScroll = True
        addOnPanel.BackColor = Color.FromArgb(248, 249, 250)
        editForm.Controls.Add(addOnPanel)

        Dim addOnCheckboxes As New List(Of CheckBox)()
        Dim addOnQuantities As New List(Of NumericUpDown)()
        Dim addOnYPos As Integer = 10

        ' Parse existing add-ons with quantities
        Dim existingAddOns As New Dictionary(Of String, Integer)()
        If Not String.IsNullOrEmpty(booking.AddOns) AndAlso booking.AddOns <> "None" Then
            Dim addOnParts = booking.AddOns.Split(","c)
            For Each part In addOnParts
                Dim trimmedPart = part.Trim()
                If trimmedPart.Contains("x") Then
                    Dim quantityPart = trimmedPart.Split("x"c)
                    If quantityPart.Length = 2 Then
                        Dim quantity As Integer
                        If Integer.TryParse(quantityPart(0).Trim(), quantity) Then
                            existingAddOns(quantityPart(1).Trim()) = quantity
                        End If
                    End If
                Else
                    existingAddOns(trimmedPart) = 1
                End If
            Next
        End If

        ' Create checkboxes and quantity controls for each add-on
        For Each addOn In availableAddOns
            ' Create checkbox
            Dim chkAddOn As New CheckBox()
            chkAddOn.Text = $"{addOn.AddOnName} (₱{addOn.Price:N2})"
            chkAddOn.Location = New Point(10, addOnYPos)
            chkAddOn.Size = New Size(250, 20)
            chkAddOn.Tag = addOn
            chkAddOn.Checked = existingAddOns.ContainsKey(addOn.AddOnName)
            addOnPanel.Controls.Add(chkAddOn)
            addOnCheckboxes.Add(chkAddOn)

            ' Create quantity control
            Dim nudQuantity As New NumericUpDown()
            nudQuantity.Location = New Point(270, addOnYPos)
            nudQuantity.Size = New Size(60, 20)
            nudQuantity.Minimum = 1
            nudQuantity.Maximum = 100
            nudQuantity.Value = If(existingAddOns.ContainsKey(addOn.AddOnName), existingAddOns(addOn.AddOnName), 1)
            nudQuantity.Enabled = chkAddOn.Checked
            addOnPanel.Controls.Add(nudQuantity)
            addOnQuantities.Add(nudQuantity)

            ' Enable/disable quantity based on checkbox state
            AddHandler chkAddOn.CheckedChanged, Sub(sender, e)
                                                    nudQuantity.Enabled = chkAddOn.Checked
                                                    If Not chkAddOn.Checked Then
                                                        nudQuantity.Value = 1
                                                    End If
                                                End Sub

            addOnYPos += 30
        Next

        yPos += 220

        ' Current Price Display
        Dim lblCurrentPrice As New Label()
        lblCurrentPrice.Text = $"Current Price: ₱{booking.TotalPrice:N2}"
        lblCurrentPrice.Location = New Point(20, yPos)
        lblCurrentPrice.Size = New Size(200, 25)
        lblCurrentPrice.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        lblCurrentPrice.ForeColor = Color.FromArgb(76, 175, 80)
        editForm.Controls.Add(lblCurrentPrice)

        yPos += 40

        ' Update price calculation
        Dim UpdatePrice As Action = Sub()
                                        Try
                                            ' Get the original price per person from the database
                                            Dim pricePerPerson As Decimal = GetPackageBasePrice(booking.PackageID)

                                            ' If we couldn't get the price from DB, calculate it from the original booking
                                            If pricePerPerson = 0 Then
                                                pricePerPerson = booking.TotalPrice / booking.NumberOfPeople
                                            End If

                                            ' Calculate new base price based on number of people
                                            Dim newPrice As Decimal = pricePerPerson * nudPeople.Value

                                            ' Add selected add-ons with their quantities
                                            For i As Integer = 0 To Math.Min(addOnCheckboxes.Count - 1, addOnQuantities.Count - 1)
                                                If addOnCheckboxes(i).Checked Then
                                                    Dim addOn As PackageAddOns = CType(addOnCheckboxes(i).Tag, PackageAddOns)
                                                    newPrice += addOn.Price * addOnQuantities(i).Value
                                                End If
                                            Next

                                            lblCurrentPrice.Text = $"Updated Price: ₱{newPrice:N2}"
                                        Catch ex As Exception
                                            lblCurrentPrice.Text = $"Current Price: ₱{booking.TotalPrice:N2}"
                                        End Try
                                    End Sub

        ' Add event handlers for price updates
        AddHandler nudPeople.ValueChanged, Sub(sender, e) UpdatePrice()
        For Each chk In addOnCheckboxes
            AddHandler chk.CheckedChanged, Sub(sender, e) UpdatePrice()
        Next
        For Each nud In addOnQuantities
            AddHandler nud.ValueChanged, Sub(sender, e) UpdatePrice()
        Next

        ' Buttons
        Dim btnSave As New Button()
        btnSave.Text = "Save"
        btnSave.Location = New Point(250, yPos)
        btnSave.Size = New Size(80, 30)
        btnSave.BackColor = Color.FromArgb(76, 175, 80)
        btnSave.ForeColor = Color.White
        btnSave.FlatStyle = FlatStyle.Flat
        AddHandler btnSave.Click, Sub()
                                      Try
                                          ' Get the original price per person
                                          Dim pricePerPerson As Decimal = GetPackageBasePrice(booking.PackageID)
                                          If pricePerPerson = 0 Then
                                              pricePerPerson = booking.TotalPrice / booking.NumberOfPeople
                                          End If

                                          ' Update booking details
                                          booking.NumberOfPeople = CInt(nudPeople.Value)
                                          booking.BookingDate = dtpDate.Value

                                          ' Build add-ons string with quantities
                                          Dim selectedAddOns As New List(Of String)()
                                          For i As Integer = 0 To Math.Min(addOnCheckboxes.Count - 1, addOnQuantities.Count - 1)
                                              If addOnCheckboxes(i).Checked Then
                                                  Dim addOn As PackageAddOns = CType(addOnCheckboxes(i).Tag, PackageAddOns)
                                                  Dim quantity As Integer = CInt(addOnQuantities(i).Value)
                                                  selectedAddOns.Add($"{quantity}x {addOn.AddOnName}")
                                              End If
                                          Next
                                          booking.AddOns = If(selectedAddOns.Count > 0, String.Join(", ", selectedAddOns), "None")

                                          ' Calculate new total price
                                          Dim newPrice As Decimal = pricePerPerson * booking.NumberOfPeople
                                          For i As Integer = 0 To Math.Min(addOnCheckboxes.Count - 1, addOnQuantities.Count - 1)
                                              If addOnCheckboxes(i).Checked Then
                                                  Dim addOn As PackageAddOns = CType(addOnCheckboxes(i).Tag, PackageAddOns)
                                                  Dim quantity As Integer = CInt(addOnQuantities(i).Value)
                                                  newPrice += addOn.Price * quantity
                                              End If
                                          Next
                                          booking.TotalPrice = newPrice

                                          editForm.DialogResult = DialogResult.OK
                                          editForm.Close()
                                      Catch ex As Exception
                                          MessageBox.Show("Error saving booking: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                      End Try
                                  End Sub
        editForm.Controls.Add(btnSave)

        Dim btnCancel As New Button()
        btnCancel.Text = "Cancel"
        btnCancel.Location = New Point(340, yPos)
        btnCancel.Size = New Size(80, 30)
        btnCancel.BackColor = Color.FromArgb(244, 67, 54)
        btnCancel.ForeColor = Color.White
        btnCancel.FlatStyle = FlatStyle.Flat
        AddHandler btnCancel.Click, Sub()
                                        editForm.DialogResult = DialogResult.Cancel
                                        editForm.Close()
                                    End Sub
        editForm.Controls.Add(btnCancel)

        If editForm.ShowDialog() = DialogResult.OK Then
            ' Force refresh of the entire tab display
            CreatePackageTypeTabs()
            MessageBox.Show("Booking updated successfully.", "Update Complete",
          MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If

        editForm.Dispose()
    End Sub

    Private Sub btnSubmitSelected_Click(sender As Object, e As EventArgs) Handles btnSubmitSelected.Click
        Dim selectedBookings As New List(Of BookingForm.BookingItem)()

        ' Collect selected bookings
        For Each tabPage As TabPage In tabControl.TabPages
            For Each control As Control In tabPage.Controls
                If TypeOf control Is DataGridView Then
                    Dim dgv As DataGridView = CType(control, DataGridView)
                    For Each row As DataGridViewRow In dgv.Rows
                        If CBool(row.Cells("Select").Value) Then
                            Dim uniqueID = row.Cells("UniqueID").Value.ToString()
                            Dim booking = BookingForm.BookingList.FirstOrDefault(Function(b) b.UniqueID = uniqueID)
                            If booking IsNot Nothing Then
                                selectedBookings.Add(booking)
                            End If
                        End If
                    Next
                End If
            Next
        Next

        If selectedBookings.Count = 0 Then
            MessageBox.Show("No bookings selected for submission.", "No Selection",
                          MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' Show confirmation preview with promo discount option
        ShowBatchBookingConfirmationPreview(selectedBookings)
    End Sub

    Private Sub LoadUserPromoDiscounts()
        If CurrentUserID <= 0 Then Return

        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()

                ' Load user's approved promo discounts
                availablePromos.Clear()
                Dim userPromoQuery As String = "SELECT pd.PromoID, pd.PromoTypeID, pt.PromoTypeName, pt.DiscountPercentage, pd.DocumentNumber " &
                     "FROM PromoDiscounts pd " &
                     "INNER JOIN PromoTypes pt ON pd.PromoTypeID = pt.PromoTypeID " &
                     "WHERE pd.UserID = @UserID AND pd.Status = 'Approved' AND pt.IsActive = 1 " &
                     "AND (pd.ExpiryDate IS NULL OR pd.ExpiryDate > CURDATE())"

                Using cmd As New MySqlCommand(userPromoQuery, conn)
                    cmd.Parameters.AddWithValue("@UserID", CurrentUserID)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim promo As New PromoDiscounts() With {
                        .PromoID = Convert.ToInt32(reader("PromoID")),
                        .PromoTypeID = Convert.ToInt32(reader("PromoTypeID")),
                        .PromoTypeName = reader("PromoTypeName").ToString(),
                        .DiscountPercentage = Convert.ToDecimal(reader("DiscountPercentage")),
                        .DocumentNumber = reader("DocumentNumber").ToString()
                    }
                            availablePromos.Add(promo)
                        End While
                    End Using
                End Using

                ' Load all available promo types
                allPromoTypes.Clear()
                Dim allPromoQuery As String = "SELECT PromoTypeID, PromoTypeName, DiscountPercentage FROM PromoTypes WHERE IsActive = 1"
                Using cmd As New MySqlCommand(allPromoQuery, conn)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim promoType As New PromoDiscounts() With {
                        .PromoTypeID = Convert.ToInt32(reader("PromoTypeID")),
                        .PromoTypeName = reader("PromoTypeName").ToString(),
                        .DiscountPercentage = Convert.ToDecimal(reader("DiscountPercentage"))
                    }
                            allPromoTypes.Add(promoType)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine("Error loading promo discounts: " & ex.Message)
            availablePromos.Clear()
            allPromoTypes.Clear()
        End Try
    End Sub

    Private Sub LoadAddOnsForPackage(packageID As Integer)
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim query As String = "SELECT * FROM PackageAddOns WHERE PackageID = @PackageID AND IsActive = 1 ORDER BY AddOnName"
                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@PackageID", packageID)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        availableAddOns.Clear()
                        While reader.Read()
                            Dim addOn As New PackageAddOns() With {
                            .AddOnID = reader("AddOnID"),
                            .packageID = reader("PackageID"),
                            .AddOnName = reader("AddOnName").ToString(),
                            .Price = Convert.ToDecimal(reader("Price")),
                            .Unit = If(IsDBNull(reader("Unit")), "", reader("Unit").ToString())
                        }
                            availableAddOns.Add(addOn)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading add-ons: " & ex.Message, "Database Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error)
            availableAddOns.Clear()
        End Try
    End Sub

    Private Function GetPackageBasePrice(packageID As Integer) As Decimal
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim query As String = "SELECT Price FROM Packages WHERE PackageID = @PackageID"
                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@PackageID", packageID)
                    Dim result = cmd.ExecuteScalar()
                    If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                        Return Convert.ToDecimal(result)
                    End If
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine("Error getting package base price: " & ex.Message)
        End Try

        ' Fallback: calculate from existing booking data
        Dim existingBooking = BookingForm.BookingList.FirstOrDefault(Function(b) b.PackageID = packageID)
        If existingBooking IsNot Nothing Then
            Return existingBooking.TotalPrice / existingBooking.NumberOfPeople
        End If

        Return 0
    End Function

    Private Sub LoadCurrentUserInfo()
        If CurrentUserID <= 0 Then
            Try
                Using conn As New MySqlConnection(connectionString)
                    conn.Open()
                    Dim query As String = "SELECT UserID, CONCAT(FirstName, ' ', LastName) as FullName, Email, Phone FROM Users WHERE IsActive = 1 ORDER BY UserID DESC LIMIT 1"
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            If reader.Read() Then
                                CurrentUserID = Convert.ToInt32(reader("UserID"))
                                CurrentUserName = reader("FullName").ToString()
                                CurrentUserEmail = reader("Email").ToString()
                                CurrentUserPhone = If(IsDBNull(reader("Phone")), "", reader("Phone").ToString())
                            End If
                        End Using
                    End Using
                End Using

                ' Ensure user exists in database - create if needed
                If CurrentUserID <= 0 Then
                    EnsureUserExists()
                End If

            Catch ex As Exception
                Console.WriteLine("Error loading current user info: " & ex.Message)
                EnsureUserExists()
            End Try
        End If

        LoadUserPromoDiscounts()
    End Sub

    Private Sub EnsureUserExists()
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim insertQuery As String = "INSERT INTO Users (Username, Email, FirstName, LastName, Password, Role, IsActive) VALUES (@Username, @Email, @FirstName, @LastName, @Password, 'User', 1)"
                Using cmd As New MySqlCommand(insertQuery, conn)
                    cmd.Parameters.AddWithValue("@Username", "guest_user")
                    cmd.Parameters.AddWithValue("@Email", "guest@lakbayph.com")
                    cmd.Parameters.AddWithValue("@FirstName", "Guest")
                    cmd.Parameters.AddWithValue("@LastName", "User")
                    cmd.Parameters.AddWithValue("@Password", "defaultpass")
                    cmd.ExecuteNonQuery()

                    CurrentUserID = CInt(cmd.LastInsertedId)
                    CurrentUserName = "Guest User"
                    CurrentUserEmail = "guest@lakbayph.com"
                    CurrentUserPhone = "N/A"
                End Using
            End Using
        Catch ex As Exception
            CurrentUserID = 1
            CurrentUserName = "Guest User"
            CurrentUserEmail = "guest@lakbayph.com"
            CurrentUserPhone = "N/A"
        End Try
    End Sub



    Private Sub ShowBatchBookingConfirmationPreview(selectedBookings As List(Of BookingForm.BookingItem))
        ' Calculate totals
        Dim originalTotal As Decimal = selectedBookings.Sum(Function(b) b.TotalPrice)
        Dim discountAmount As Decimal = 0
        Dim finalTotal As Decimal = originalTotal

        ' Create confirmation preview form
        Dim previewForm As New Form()
        previewForm.Text = "Confirm Batch Booking Submission"
        previewForm.Size = New Size(700, 800)
        previewForm.StartPosition = FormStartPosition.CenterParent
        previewForm.FormBorderStyle = FormBorderStyle.FixedDialog
        previewForm.MaximizeBox = False
        previewForm.MinimizeBox = False
        previewForm.BackColor = Color.White

        ' Header Panel
        Dim headerPanel As New Panel()
        headerPanel.Location = New Point(0, 0)
        headerPanel.Size = New Size(700, 80)
        headerPanel.BackColor = Color.FromArgb(25, 118, 210)
        previewForm.Controls.Add(headerPanel)

        Dim headerLabel As New Label()
        headerLabel.Text = $"Confirm Submission - {selectedBookings.Count} Booking(s)"
        headerLabel.Location = New Point(30, 25)
        headerLabel.Size = New Size(500, 30)
        headerLabel.Font = New Font("Segoe UI", 16, FontStyle.Bold)
        headerLabel.ForeColor = Color.White
        headerPanel.Controls.Add(headerLabel)

        ' Content Panel
        Dim contentPanel As New Panel()
        contentPanel.Location = New Point(30, 100)
        contentPanel.Size = New Size(640, 550)
        contentPanel.BackColor = Color.FromArgb(248, 249, 250)
        contentPanel.BorderStyle = BorderStyle.FixedSingle
        contentPanel.AutoScroll = True
        previewForm.Controls.Add(contentPanel)

        Dim yPos As Integer = 20

        ' Customer Information
        yPos = CreateConfirmationSection(contentPanel, "CUSTOMER INFORMATION", yPos)
        yPos = CreateConfirmationRow(contentPanel, "Name:", If(String.IsNullOrEmpty(CurrentUserName), "Not Available", CurrentUserName), yPos)
        yPos = CreateConfirmationRow(contentPanel, "Email:", If(String.IsNullOrEmpty(CurrentUserEmail), "Not Available", CurrentUserEmail), yPos)
        yPos += 30

        ' Selected Bookings
        yPos = CreateConfirmationSection(contentPanel, "SELECTED BOOKINGS", yPos)
        For i As Integer = 0 To selectedBookings.Count - 1
            Dim booking = selectedBookings(i)
            yPos = CreateConfirmationRow(contentPanel, $"Booking {i + 1}:", booking.PackageName, yPos)
            yPos = CreateConfirmationRow(contentPanel, "People:", booking.NumberOfPeople.ToString(), yPos)
            yPos = CreateConfirmationRow(contentPanel, "Date:", booking.BookingDate.ToString("MMM dd, yyyy"), yPos)
            yPos = CreateConfirmationRow(contentPanel, "Add-ons:", booking.AddOns, yPos)
            yPos = CreateConfirmationRow(contentPanel, "Amount:", $"₱{booking.TotalPrice:N2}", yPos, True)
            yPos += 15
        Next

        ' Promo Discount Section
        yPos = CreateConfirmationSection(contentPanel, "DISCOUNT OPTIONS", yPos)

        Dim lblPromo As New Label()
        lblPromo.Text = "Select Discount (Optional):"
        lblPromo.Location = New Point(20, yPos)
        lblPromo.Size = New Size(180, 20)
        lblPromo.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        contentPanel.Controls.Add(lblPromo)

        Dim cmbPromo As New ComboBox()
        cmbPromo.Location = New Point(210, yPos)
        cmbPromo.Size = New Size(250, 25)
        cmbPromo.DropDownStyle = ComboBoxStyle.DropDownList
        cmbPromo.Items.Add("-- No Discount --")

        ' Add all promo types with eligibility indication
        For Each promo In allPromoTypes
            cmbPromo.Items.Add($"✓ {promo.PromoTypeName} ({promo.DiscountPercentage}% off)")
        Next

        cmbPromo.SelectedIndex = 0
        contentPanel.Controls.Add(cmbPromo)

        yPos += 40

        ' Pricing Section
        yPos = CreateConfirmationSection(contentPanel, "PRICING SUMMARY", yPos)

        Dim lblOriginal As New Label()
        lblOriginal.Text = $"Original Total: ₱{originalTotal:N2}"
        lblOriginal.Location = New Point(20, yPos)
        lblOriginal.Size = New Size(300, 25)
        lblOriginal.Font = New Font("Segoe UI", 12)
        contentPanel.Controls.Add(lblOriginal)
        yPos += 30

        Dim lblDiscount As New Label()
        lblDiscount.Text = "Discount: ₱0.00"
        lblDiscount.Location = New Point(20, yPos)
        lblDiscount.Size = New Size(300, 25)
        lblDiscount.Font = New Font("Segoe UI", 12)
        lblDiscount.ForeColor = Color.FromArgb(255, 87, 34)
        contentPanel.Controls.Add(lblDiscount)
        yPos += 30

        Dim lblFinal As New Label()
        lblFinal.Text = $"Final Total: ₱{finalTotal:N2}"
        lblFinal.Location = New Point(20, yPos)
        lblFinal.Size = New Size(300, 30)
        lblFinal.Font = New Font("Segoe UI", 14, FontStyle.Bold)
        lblFinal.ForeColor = Color.FromArgb(76, 175, 80)
        contentPanel.Controls.Add(lblFinal)

        ' Update pricing when promo changes
        AddHandler cmbPromo.SelectedIndexChanged, Sub()
                                                      If cmbPromo.SelectedIndex > 0 Then
                                                          Dim selectedPromoType = allPromoTypes(cmbPromo.SelectedIndex - 1)
                                                          Dim userPromo = availablePromos.FirstOrDefault(Function(p) p.PromoTypeID = selectedPromoType.PromoTypeID)
                                                          If userPromo IsNot Nothing Then
                                                              discountAmount = Math.Round(originalTotal * (userPromo.DiscountPercentage / 100), 2)
                                                              appliedPromoDetails = userPromo
                                                              selectedPromoID = userPromo.PromoID
                                                          Else
                                                              ' User doesn't have this promo, but show the discount calculation anyway
                                                              discountAmount = Math.Round(originalTotal * (selectedPromoType.DiscountPercentage / 100), 2)
                                                              appliedPromoDetails = selectedPromoType
                                                              selectedPromoID = 0
                                                          End If
                                                      Else
                                                          discountAmount = 0
                                                          appliedPromoDetails = Nothing
                                                          selectedPromoID = 0
                                                      End If

                                                      finalTotal = originalTotal - discountAmount
                                                      lblDiscount.Text = $"Discount: -₱{discountAmount:N2}"
                                                      lblFinal.Text = $"Final Total: ₱{finalTotal:N2}"
                                                      lblFinal.ForeColor = If(discountAmount > 0, Color.FromArgb(34, 139, 34), Color.FromArgb(76, 175, 80))
                                                  End Sub

        ' Buttons Panel
        Dim buttonPanel As New Panel()
        buttonPanel.Location = New Point(0, 700)
        buttonPanel.Size = New Size(700, 80)
        buttonPanel.BackColor = Color.FromArgb(248, 249, 250)
        previewForm.Controls.Add(buttonPanel)

        ' Cancel Button
        Dim btnCancel As New Button()
        btnCancel.Text = "Cancel"
        btnCancel.Location = New Point(250, 20)
        btnCancel.Size = New Size(120, 35)
        btnCancel.BackColor = Color.FromArgb(244, 67, 54)
        btnCancel.ForeColor = Color.White
        btnCancel.FlatStyle = FlatStyle.Flat
        btnCancel.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        AddHandler btnCancel.Click, Sub()
                                        previewForm.DialogResult = DialogResult.Cancel
                                        previewForm.Close()
                                    End Sub
        buttonPanel.Controls.Add(btnCancel)

        ' Confirm Button
        Dim btnConfirm As New Button()
        btnConfirm.Text = "Submit All Bookings"
        btnConfirm.Location = New Point(380, 20)
        btnConfirm.Size = New Size(140, 35)
        btnConfirm.BackColor = Color.FromArgb(76, 175, 80)
        btnConfirm.ForeColor = Color.White
        btnConfirm.FlatStyle = FlatStyle.Flat
        btnConfirm.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        AddHandler btnConfirm.Click, Sub()
                                         previewForm.Close()
                                         ProcessBatchBookingSubmission(selectedBookings, finalTotal, discountAmount)
                                     End Sub
        buttonPanel.Controls.Add(btnConfirm)

        previewForm.ShowDialog()
        previewForm.Dispose()
    End Sub

    Private Sub ProcessBatchBookingSubmission(selectedBookings As List(Of BookingForm.BookingItem), finalTotal As Decimal, discountAmount As Decimal)
        btnSubmitSelected.Enabled = False
        btnSubmitSelected.Text = "Processing..."

        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Using transaction As MySqlTransaction = conn.BeginTransaction()
                    Try
                        ' Create a list to store all booking references
                        Dim allBookingReferences As New List(Of String)()

                        ' Calculate discount ratio for proportional distribution
                        Dim originalTotal As Decimal = selectedBookings.Sum(Function(b) b.TotalPrice)
                        Dim discountRatio As Decimal = If(originalTotal > 0, discountAmount / originalTotal, 0)

                        ' Insert each booking individually with unique reference
                        For Each booking In selectedBookings
                            ' Calculate this booking's share of the discount
                            Dim bookingDiscount As Decimal = Math.Round(booking.TotalPrice * discountRatio, 2)

                            ' Generate unique booking reference for each package
                            Dim bookingReference As String = GenerateUniqueBookingReference(conn, transaction)
                            allBookingReferences.Add(bookingReference)

                            ' Insert the individual booking
                            Dim bookingId As Long = InsertIndividualBooking(conn, transaction, booking, bookingReference,
                                                                      booking.TotalPrice, bookingDiscount)

                            ' Insert booking add-ons if any
                            If booking.AddOns <> "None" Then
                                InsertBookingAddOns(conn, transaction, bookingId, booking)
                            End If

                            ' Insert booking history for this individual booking
                            InsertBookingHistory(conn, transaction, bookingId, "Created",
                                           $"Booking created with reference {bookingReference}")
                        Next

                        ' Create a batch note in the first booking to link them
                        If allBookingReferences.Count > 0 Then
                            AddBatchNoteToFirstBooking(conn, transaction, allBookingReferences.First(),
                                                 allBookingReferences.Count, finalTotal, discountAmount)
                        End If

                        transaction.Commit()

                        ' Remove submitted bookings from the list
                        For Each booking In selectedBookings
                            BookingForm.BookingList.RemoveAll(Function(b) b.UniqueID = booking.UniqueID)
                        Next

                        ' Refresh the display
                        CreatePackageTypeTabs()

                        ' Show success confirmation
                        ShowBatchSubmissionConfirmation(allBookingReferences, selectedBookings, finalTotal, discountAmount)

                    Catch ex As Exception
                        Try
                            transaction.Rollback()
                        Catch rollbackEx As Exception
                            Console.WriteLine("Rollback error: " & rollbackEx.Message)
                        End Try
                        MessageBox.Show("Error submitting bookings: " & ex.Message, "Submission Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End Try
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error connecting to database: " & ex.Message, "Database Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            btnSubmitSelected.Enabled = True
            btnSubmitSelected.Text = "Submit Selected"
        End Try
    End Sub

    Private Sub InsertBatchSummary(conn As MySqlConnection, transaction As MySqlTransaction,
                             batchReference As String, bookingReferences As List(Of String),
                             totalAmount As Decimal, totalDiscount As Decimal, bookingCount As Integer)
        Try
            ' Create a simple note in the first booking to link the batch
            Dim firstBookingRef = bookingReferences.First()
            Dim updateQuery As String = "UPDATE Bookings SET Notes = CONCAT(Notes, @BatchNote) 
                                   WHERE BookingReference = @BookingReference"

            Using cmd As New MySqlCommand(updateQuery, conn, transaction)
                cmd.Parameters.AddWithValue("@BookingReference", firstBookingRef)
                cmd.Parameters.AddWithValue("@BatchNote", $"{vbCrLf}{vbCrLf}BATCH: {batchReference} ({bookingCount} bookings)")
                cmd.ExecuteNonQuery()
            End Using

            ' Alternatively, you could create a separate batch table if needed
            ' This matches your database schema which doesn't store totals
        Catch ex As Exception
            Console.WriteLine("Warning: Could not update batch summary note: " & ex.Message)
        End Try
    End Sub
    Private Function GenerateUniqueBookingReference(conn As MySqlConnection, transaction As MySqlTransaction) As String
        Try
            ' Get the next sequence number
            Dim query As String = "SELECT COALESCE(MAX(CAST(SUBSTRING(BookingReference, 5) AS UNSIGNED)), 0) + 1 
                             FROM Bookings 
                             WHERE BookingReference REGEXP '^LKB[0-9]+$'"

            Using cmd As New MySqlCommand(query, conn, transaction)
                Dim nextNumber = cmd.ExecuteScalar()
                Return $"LKB{nextNumber.ToString().PadLeft(8, "0"c)}"
            End Using
        Catch ex As Exception
            ' Fallback if there's an error
            Return $"LKB{DateTime.Now.ToString("yyyyMMddHHmmss")}"
        End Try
    End Function
    Private Sub AddBatchNoteToFirstBooking(conn As MySqlConnection, transaction As MySqlTransaction,
                                     firstBookingRef As String, bookingCount As Integer,
                                     finalTotal As Decimal, discountAmount As Decimal)
        Try
            Dim updateQuery As String = "UPDATE Bookings SET Notes = CONCAT(COALESCE(Notes, ''), @BatchNote) 
                                   WHERE BookingReference = @BookingReference"

            Dim batchNote As String = $"{vbCrLf}{vbCrLf}=== BATCH DETAILS ===" &
                                 $"{vbCrLf}Total Bookings: {bookingCount}" &
                                 $"{vbCrLf}Original Total: ₱{selectedBookings.Sum(Function(b) b.TotalPrice):N2}"

            If discountAmount > 0 Then
                batchNote &= $"{vbCrLf}Discount Applied: ₱{discountAmount:N2}"
                If appliedPromoDetails IsNot Nothing Then
                    batchNote &= $" ({appliedPromoDetails.PromoTypeName})"
                End If
            End If

            batchNote &= $"{vbCrLf}Final Total: ₱{finalTotal:N2}"

            Using cmd As New MySqlCommand(updateQuery, conn, transaction)
                cmd.Parameters.AddWithValue("@BookingReference", firstBookingRef)
                cmd.Parameters.AddWithValue("@BatchNote", batchNote)
                cmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            Console.WriteLine("Warning: Could not update batch note: " & ex.Message)
        End Try
    End Sub
    Private Function InsertIndividualBooking(conn As MySqlConnection, transaction As MySqlTransaction,
                                       booking As BookingForm.BookingItem, bookingReference As String,
                                       originalAmount As Decimal, discountAmount As Decimal) As Long
        Try
            Dim bookingId As Long = 0

            Dim insertQuery As String = "INSERT INTO Bookings (
            BookingReference, UserID, PackageID, NumberOfPeople, PromoID, 
            TravelDate, EndDate, BookingDate, Status, Notes
        ) VALUES (
            @BookingReference, @UserID, @PackageID, @NumberOfPeople, @PromoID, 
            @TravelDate, @EndDate, @BookingDate, @Status, @Notes
        )"

            Using cmd As New MySqlCommand(insertQuery, conn, transaction)
                cmd.Parameters.AddWithValue("@BookingReference", bookingReference)
                cmd.Parameters.AddWithValue("@UserID", CurrentUserID)
                cmd.Parameters.AddWithValue("@PackageID", booking.PackageID)
                cmd.Parameters.AddWithValue("@NumberOfPeople", booking.NumberOfPeople)
                cmd.Parameters.AddWithValue("@PromoID", If(selectedPromoID > 0, selectedPromoID, DBNull.Value))
                cmd.Parameters.AddWithValue("@TravelDate", booking.BookingDate)
                cmd.Parameters.AddWithValue("@EndDate", booking.BookingDate)
                cmd.Parameters.AddWithValue("@BookingDate", DateTime.Now)
                cmd.Parameters.AddWithValue("@Status", "Pending")

                ' Create detailed notes
                Dim notesBuilder As New StringBuilder()
                notesBuilder.AppendLine($"Package: {booking.PackageName}")
                notesBuilder.AppendLine($"People: {booking.NumberOfPeople}")
                notesBuilder.AppendLine($"Date: {booking.BookingDate:MMM dd, yyyy}")
                notesBuilder.AppendLine($"Add-ons: {booking.AddOns}")
                notesBuilder.AppendLine($"Original Amount: ₱{originalAmount:N2}")

                If discountAmount > 0 Then
                    notesBuilder.AppendLine($"Discount: ₱{discountAmount:N2}")
                    If appliedPromoDetails IsNot Nothing Then
                        notesBuilder.AppendLine($"Promo: {appliedPromoDetails.PromoTypeName}")
                    End If
                End If

                Dim noteText As String = notesBuilder.ToString()
                cmd.Parameters.AddWithValue("@Notes", noteText)

                cmd.ExecuteNonQuery()
                bookingId = cmd.LastInsertedId
            End Using

            ' Insert payment record
            InsertBookingPayment(conn, transaction, bookingId)

            Return bookingId

        Catch ex As Exception
            Console.WriteLine("Error inserting individual booking: " & ex.Message)
            Throw
        End Try
    End Function
    Private Sub InsertBookingPayment(conn As MySqlConnection, transaction As MySqlTransaction, bookingId As Long)
        Try
            Dim query As String = "INSERT INTO BookingPayments (
            BookingID, PaymentMethod, PaymentStatus
        ) VALUES (
            @BookingID, @PaymentMethod, @PaymentStatus
        )"

            Using cmd As New MySqlCommand(query, conn, transaction)
                cmd.Parameters.AddWithValue("@BookingID", bookingId)
                cmd.Parameters.AddWithValue("@PaymentMethod", "Pay on Trip")
                cmd.Parameters.AddWithValue("@PaymentStatus", "Pending")
                cmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            Console.WriteLine("Error inserting booking payment: " & ex.Message)
            Throw
        End Try
    End Sub
    Private Sub InsertBookingAddOns(conn As MySqlConnection, transaction As MySqlTransaction,
                              bookingId As Long, booking As BookingForm.BookingItem)
        Try
            ' Parse add-ons string (format: "2x Extra Dive, 1x Equipment Rental")
            Dim addOnParts = booking.AddOns.Split(","c)

            For Each part In addOnParts
                Dim trimmedPart = part.Trim()
                If trimmedPart.Contains("x") Then
                    Dim quantityPart = trimmedPart.Split("x"c)
                    If quantityPart.Length = 2 Then
                        Dim quantity As Integer
                        Dim addOnName = quantityPart(1).Trim()

                        If Integer.TryParse(quantityPart(0).Trim(), quantity) Then
                            ' Get add-on details from database
                            Dim addOn = GetAddOnByName(conn, transaction, booking.PackageID, addOnName)

                            If addOn IsNot Nothing Then
                                Dim query As String = "INSERT INTO BookingAddOns (
                                BookingID, AddOnID, Quantity, UnitPrice
                            ) VALUES (
                                @BookingID, @AddOnID, @Quantity, @UnitPrice
                            )"

                                Using cmd As New MySqlCommand(query, conn, transaction)
                                    cmd.Parameters.AddWithValue("@BookingID", bookingId)
                                    cmd.Parameters.AddWithValue("@AddOnID", addOn.AddOnID)
                                    cmd.Parameters.AddWithValue("@Quantity", quantity)
                                    cmd.Parameters.AddWithValue("@UnitPrice", addOn.Price)
                                    cmd.ExecuteNonQuery()
                                End Using
                            End If
                        End If
                    End If
                End If
            Next
        Catch ex As Exception
            Console.WriteLine("Error inserting booking add-ons: " & ex.Message)
            Throw
        End Try
    End Sub

    Private Function GetNextBookingId(conn As MySqlConnection, transaction As MySqlTransaction) As Long
        Try
            Dim currentYear As String = DateTime.Now.Year.ToString()
            Dim checkColumnQuery As String = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Bookings' AND COLUMN_NAME = 'BookingReference'"

            Using checkCmd As New MySqlCommand(checkColumnQuery, conn, transaction)
                Dim columnExists As Long = CLng(checkCmd.ExecuteScalar())

                If columnExists > 0 Then
                    Dim getMaxQuery As String = $"SELECT COALESCE(MAX(CAST(SUBSTRING(BookingReference, LOCATE('-', BookingReference, LOCATE('-', BookingReference) + 1) + 1) AS UNSIGNED)), 0) + 1 FROM Bookings WHERE BookingReference LIKE 'LKB-{currentYear}-%'"
                    Using cmd As New MySqlCommand(getMaxQuery, conn, transaction)
                        Dim nextNumber = cmd.ExecuteScalar()
                        If nextNumber IsNot Nothing AndAlso Not IsDBNull(nextNumber) Then
                            Return CLng(nextNumber)
                        Else
                            Return 1
                        End If
                    End Using
                Else
                    Dim getMaxIdQuery As String = "SELECT COALESCE(MAX(BookingID), 0) + 1 FROM Bookings"
                    Using cmd As New MySqlCommand(getMaxIdQuery, conn, transaction)
                        Dim nextNumber = cmd.ExecuteScalar()
                        If nextNumber IsNot Nothing AndAlso Not IsDBNull(nextNumber) Then
                            Return CLng(nextNumber)
                        Else
                            Return 1
                        End If
                    End Using
                End If
            End Using
        Catch ex As Exception
            Return (DateTime.Now.Ticks Mod 1000000) + 1
        End Try
    End Function
    Private Function GetAddOnByName(conn As MySqlConnection, transaction As MySqlTransaction,
                              packageId As Integer, addOnName As String) As PackageAddOns
        Try
            Dim query As String = "SELECT AddOnID, PackageID, AddOnName, Price, Unit 
                             FROM PackageAddOns 
                             WHERE PackageID = @PackageID AND AddOnName = @AddOnName"

            Using cmd As New MySqlCommand(query, conn, transaction)
                cmd.Parameters.AddWithValue("@PackageID", packageId)
                cmd.Parameters.AddWithValue("@AddOnName", addOnName)

                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        Return New PackageAddOns() With {
                        .AddOnID = reader.GetInt32("AddOnID"),
                        .packageID = reader.GetInt32("PackageID"),
                        .AddOnName = reader.GetString("AddOnName"),
                        .Price = reader.GetDecimal("Price"),
                        .Unit = If(reader.IsDBNull("Unit"), "", reader.GetString("Unit"))
                    }
                    End If
                End Using
            End Using

            Return Nothing
        Catch ex As Exception
            Console.WriteLine("Error getting add-on by name: " & ex.Message)
            Return Nothing
        End Try
    End Function
    Private Sub InsertBatchBookingSummary(conn As MySqlConnection, transaction As MySqlTransaction,
                                    batchReference As String, bookingReferences As List(Of String),
                                    totalAmount As Decimal, totalDiscount As Decimal, bookingCount As Integer)
        Try
            ' Check if BatchBookings table exists
            Dim tableExistsQuery As String = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'BatchBookings'"
            Using checkCmd As New MySqlCommand(tableExistsQuery, conn, transaction)
                Dim tableExists As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())

                If tableExists > 0 Then
                    Dim batchQuery As String = "INSERT INTO BatchBookings (BatchReference, BookingReferences, TotalBookings, 
                                          TotalAmount, TotalDiscount, CreatedDate, UserID, Status) 
                                          VALUES (@BatchReference, @BookingReferences, @TotalBookings, 
                                          @TotalAmount, @TotalDiscount, @CreatedDate, @UserID, @Status)"

                    Using cmd As New MySqlCommand(batchQuery, conn, transaction)
                        cmd.Parameters.AddWithValue("@BatchReference", batchReference)
                        cmd.Parameters.AddWithValue("@BookingReferences", String.Join(",", bookingReferences))
                        cmd.Parameters.AddWithValue("@TotalBookings", bookingCount)
                        cmd.Parameters.AddWithValue("@TotalAmount", totalAmount)
                        cmd.Parameters.AddWithValue("@TotalDiscount", totalDiscount)
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now)
                        cmd.Parameters.AddWithValue("@UserID", CurrentUserID)
                        cmd.Parameters.AddWithValue("@Status", "Pending")
                        cmd.ExecuteNonQuery()
                    End Using
                End If
            End Using
        Catch ex As Exception
            Console.WriteLine("Warning: Could not insert batch summary: " & ex.Message)
        End Try
    End Sub

    Private Function InsertBatchBooking(conn As MySqlConnection, transaction As MySqlTransaction, selectedBookings As List(Of BookingForm.BookingItem), bookingReference As String, finalTotal As Decimal, discountAmount As Decimal) As Long
        Try
            Dim bookingId As Long = 0

            ' Create consolidated booking details
            Dim packageNames = String.Join(", ", selectedBookings.Select(Function(b) b.PackageName).Distinct())
            Dim totalPeople = selectedBookings.Sum(Function(b) b.NumberOfPeople)
            Dim earliestDate = selectedBookings.Min(Function(b) b.BookingDate)
            Dim latestDate = selectedBookings.Max(Function(b) b.BookingDate)
            Dim allAddOns = String.Join(" | ", selectedBookings.Select(Function(b) $"{b.PackageName}: {b.AddOns}").Where(Function(s) Not s.EndsWith(": None")))

            ' Get the first package ID for the main booking record
            Dim mainPackageID = selectedBookings.First().PackageID

            Dim insertQuery As String = "INSERT INTO Bookings (UserID, PackageID, NumberOfPeople, PromoID, TravelDate, EndDate, BookingDate, Status, BookingReference, Notes) " &
                                "VALUES (@UserID, @PackageID, @NumberOfPeople, @PromoID, @TravelDate, @EndDate, @BookingDate, @Status, @BookingReference, @Notes)"

            Using cmd As New MySqlCommand(insertQuery, conn, transaction)
                cmd.Parameters.Add("@UserID", MySqlDbType.Int32).Value = CurrentUserID
                cmd.Parameters.Add("@PackageID", MySqlDbType.Int64).Value = mainPackageID
                cmd.Parameters.Add("@NumberOfPeople", MySqlDbType.Int32).Value = totalPeople
                cmd.Parameters.Add("@PromoID", MySqlDbType.Int32).Value = If(selectedPromoID > 0, selectedPromoID, DBNull.Value)
                cmd.Parameters.Add("@TravelDate", MySqlDbType.Date).Value = earliestDate
                cmd.Parameters.Add("@EndDate", MySqlDbType.Date).Value = latestDate
                cmd.Parameters.Add("@BookingDate", MySqlDbType.DateTime).Value = DateTime.Now
                cmd.Parameters.Add("@Status", MySqlDbType.VarChar, 20).Value = "Pending"
                cmd.Parameters.Add("@BookingReference", MySqlDbType.VarChar, 20).Value = bookingReference

                ' Create detailed notes with all booking info
                Dim notesBuilder As New StringBuilder()
                notesBuilder.AppendLine($"BATCH: {selectedBookings.Count} packages")
                notesBuilder.AppendLine($"Orig: ₱{selectedBookings.Sum(Function(b) b.TotalPrice):N2}")
                If discountAmount > 0 Then
                    notesBuilder.AppendLine($"Disc: ₱{discountAmount:N2}")
                End If
                notesBuilder.AppendLine($"Final: ₱{finalTotal:N2}")

                Dim noteText As String = notesBuilder.ToString()
                If noteText.Length > 255 Then
                    noteText = noteText.Substring(0, 252) + "..."
                End If

                cmd.Parameters.Add("@Notes", MySqlDbType.VarChar, 255).Value = noteText

                cmd.ExecuteNonQuery()
                bookingId = cmd.LastInsertedId
            End Using

            Return bookingId

        Catch ex As MySqlException
            MessageBox.Show("MySQL Error: " & ex.Message & vbCrLf & "Error Number: " & ex.Number, "Database Error")
            Throw
        Catch ex As Exception
            MessageBox.Show("General Error: " & ex.Message, "Error")
            Throw
        End Try
    End Function

    Private Sub InsertBatchBookingSummary(conn As MySqlConnection, transaction As MySqlTransaction, mainBookingID As Long, bookingReferences As List(Of String), totalAmount As Decimal, totalDiscount As Decimal)
        Try
            ' Check if BatchBookings table exists, if not create a simple summary record
            Dim tableExistsQuery As String = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'BatchBookings'"
            Using checkCmd As New MySqlCommand(tableExistsQuery, conn, transaction)
                Dim tableExists As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())

                If tableExists > 0 Then
                    Dim batchQuery As String = "INSERT INTO BatchBookings (MainBookingID, BookingReferences, TotalBookings, TotalAmount, TotalDiscount, CreatedDate, UserID) " &
                                              "VALUES (@MainBookingID, @BookingReferences, @TotalBookings, @TotalAmount, @TotalDiscount, @CreatedDate, @UserID)"

                    Using cmd As New MySqlCommand(batchQuery, conn, transaction)
                        cmd.Parameters.AddWithValue("@MainBookingID", mainBookingID)
                        cmd.Parameters.AddWithValue("@BookingReferences", String.Join(",", bookingReferences))
                        cmd.Parameters.AddWithValue("@TotalBookings", bookingReferences.Count)
                        cmd.Parameters.AddWithValue("@TotalAmount", totalAmount)
                        cmd.Parameters.AddWithValue("@TotalDiscount", totalDiscount)
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now)
                        cmd.Parameters.AddWithValue("@UserID", CurrentUserID)
                        cmd.ExecuteNonQuery()
                    End Using
                End If
            End Using
        Catch ex As Exception
            Console.WriteLine("Warning: Could not insert batch summary: " & ex.Message)
        End Try
    End Sub

    Private Sub InsertBookingHistory(conn As MySqlConnection, transaction As MySqlTransaction,
                               bookingId As Long, actionType As String, description As String)
        Try
            Dim query As String = "INSERT INTO BookingHistory (
            BookingID, UserID, ActionType, ActionDescription, 
            PreviousStatus, NewStatus, ModifiedBy
        ) VALUES (
            @BookingID, @UserID, @ActionType, @ActionDescription, 
            NULL, 'Pending', @UserID
        )"

            Using cmd As New MySqlCommand(query, conn, transaction)
                cmd.Parameters.AddWithValue("@BookingID", bookingId)
                cmd.Parameters.AddWithValue("@UserID", CurrentUserID)
                cmd.Parameters.AddWithValue("@ActionType", actionType)
                cmd.Parameters.AddWithValue("@ActionDescription", description)
                cmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            Console.WriteLine("Error inserting booking history: " & ex.Message)
            Throw
        End Try
    End Sub
    Private Sub ShowBatchSubmissionConfirmation(bookingReferences As List(Of String), submittedBookings As List(Of BookingForm.BookingItem), totalAmount As Decimal, discountAmount As Decimal)
        ' Create confirmation form
        Dim confirmForm As New Form()
        confirmForm.Text = "Batch Booking Confirmation"
        confirmForm.Size = New Size(700, 900)
        confirmForm.StartPosition = FormStartPosition.CenterParent
        confirmForm.FormBorderStyle = FormBorderStyle.FixedDialog
        confirmForm.MaximizeBox = False
        confirmForm.MinimizeBox = False
        confirmForm.BackColor = Color.White

        ' Success Header Panel
        Dim headerPanel As New Panel()
        headerPanel.Location = New Point(0, 0)
        headerPanel.Size = New Size(700, 100)
        headerPanel.BackColor = Color.FromArgb(34, 139, 34)
        confirmForm.Controls.Add(headerPanel)

        ' Success Icon
        Dim successIcon As New Label()
        successIcon.Text = "✓"
        successIcon.Location = New Point(50, 20)
        successIcon.Size = New Size(60, 60)
        successIcon.Font = New Font("Segoe UI", 36, FontStyle.Bold)
        successIcon.ForeColor = Color.White
        successIcon.TextAlign = ContentAlignment.MiddleCenter
        headerPanel.Controls.Add(successIcon)

        ' Success Title
        Dim successTitle As New Label()
        successTitle.Text = "Batch Booking Submitted!"
        successTitle.Location = New Point(130, 25)
        successTitle.Size = New Size(400, 35)
        successTitle.Font = New Font("Segoe UI", 20, FontStyle.Bold)
        successTitle.ForeColor = Color.White
        headerPanel.Controls.Add(successTitle)

        ' Success Subtitle
        Dim successSubtitle As New Label()
        successSubtitle.Text = $"{submittedBookings.Count} bookings are pending admin approval"
        successSubtitle.Location = New Point(130, 60)
        successSubtitle.Size = New Size(400, 20)
        successSubtitle.Font = New Font("Segoe UI", 11)
        successSubtitle.ForeColor = Color.White
        headerPanel.Controls.Add(successSubtitle)

        ' Content Panel
        Dim contentPanel As New Panel()
        contentPanel.Location = New Point(30, 120)
        contentPanel.Size = New Size(640, 650)
        contentPanel.BackColor = Color.FromArgb(248, 249, 250)
        contentPanel.BorderStyle = BorderStyle.FixedSingle
        contentPanel.AutoScroll = True
        confirmForm.Controls.Add(contentPanel)

        Dim yPos As Integer = 20

        ' Booking References Section
        yPos = CreateConfirmationSection(contentPanel, "BOOKING REFERENCES", yPos)
        For i As Integer = 0 To bookingReferences.Count - 1
            yPos = CreateConfirmationRow(contentPanel, $"Booking {i + 1}:", bookingReferences(i), yPos)
        Next
        yPos += 30

        ' Customer Information
        yPos = CreateConfirmationSection(contentPanel, "CUSTOMER INFORMATION", yPos)
        yPos = CreateConfirmationRow(contentPanel, "Name:", CurrentUserName, yPos)
        yPos = CreateConfirmationRow(contentPanel, "Email:", CurrentUserEmail, yPos)
        yPos += 30

        ' Submitted Bookings Summary
        yPos = CreateConfirmationSection(contentPanel, "SUBMITTED BOOKINGS", yPos)
        For i As Integer = 0 To submittedBookings.Count - 1
            Dim booking = submittedBookings(i)
            yPos = CreateConfirmationRow(contentPanel, $"Package {i + 1}:", booking.PackageName, yPos)
            yPos = CreateConfirmationRow(contentPanel, "People:", booking.NumberOfPeople.ToString(), yPos)
            yPos = CreateConfirmationRow(contentPanel, "Date:", booking.BookingDate.ToString("MMM dd, yyyy"), yPos)
            yPos = CreateConfirmationRow(contentPanel, "Add-ons:", booking.AddOns, yPos)
            yPos = CreateConfirmationRow(contentPanel, "Amount:", $"₱{booking.TotalPrice:N2}", yPos, True)
            yPos += 15
        Next

        ' Pricing Summary
        yPos = CreateConfirmationSection(contentPanel, "TOTAL PRICING", yPos)

        Dim originalTotal = submittedBookings.Sum(Function(b) b.TotalPrice)
        yPos = CreateConfirmationRow(contentPanel, "Original Total:", $"₱{originalTotal:N2}", yPos)

        If discountAmount > 0 AndAlso appliedPromoDetails IsNot Nothing Then
            yPos = CreateConfirmationRow(contentPanel, "Discount Applied:", $"₱{discountAmount:N2} ({appliedPromoDetails.PromoTypeName})", yPos)
        Else
            yPos = CreateConfirmationRow(contentPanel, "Discount Applied:", "None", yPos)
        End If

        yPos = CreateConfirmationRow(contentPanel, "FINAL TOTAL:", $"₱{totalAmount:N2}", yPos, True)
        yPos += 30

        ' Important Notes
        yPos = CreateConfirmationSection(contentPanel, "IMPORTANT NOTES", yPos)

        Dim notesText As String = $"• All {submittedBookings.Count} bookings are currently PENDING admin approval" & vbCrLf &
                                 "• You will receive confirmation emails for each booking once approved" & vbCrLf &
                                 "• Please keep all booking reference numbers for future inquiries" & vbCrLf &
                                 "• Contact customer service for any questions or changes"

        If discountAmount > 0 AndAlso appliedPromoDetails IsNot Nothing Then
            notesText &= vbCrLf & $"• Your {appliedPromoDetails.PromoTypeName} discount has been applied to this batch"
        End If

        Dim notesLabel As New Label()
        notesLabel.Text = notesText
        notesLabel.Location = New Point(20, yPos)
        notesLabel.Size = New Size(600, 120)
        notesLabel.Font = New Font("Segoe UI", 9)
        notesLabel.ForeColor = Color.FromArgb(100, 100, 100)
        contentPanel.Controls.Add(notesLabel)

        ' Buttons Panel
        Dim buttonPanel As New Panel()
        buttonPanel.Location = New Point(0, 790)
        buttonPanel.Size = New Size(700, 80)
        buttonPanel.BackColor = Color.FromArgb(248, 249, 250)
        confirmForm.Controls.Add(buttonPanel)

        ' Print Receipt Button
        Dim btnPrint As New Button()
        btnPrint.Text = "Print Receipt"
        btnPrint.Location = New Point(200, 20)
        btnPrint.Size = New Size(120, 35)
        btnPrint.BackColor = Color.FromArgb(70, 130, 180)
        btnPrint.ForeColor = Color.White
        btnPrint.FlatStyle = FlatStyle.Flat
        btnPrint.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        AddHandler btnPrint.Click, Sub() PrintBatchReceiptContent(bookingReferences, submittedBookings, totalAmount, discountAmount)
        buttonPanel.Controls.Add(btnPrint)

        ' Close Button
        Dim btnClose As New Button()
        btnClose.Text = "Close"
        btnClose.Location = New Point(330, 20)
        btnClose.Size = New Size(120, 35)
        btnClose.BackColor = Color.FromArgb(34, 139, 34)
        btnClose.ForeColor = Color.White
        btnClose.FlatStyle = FlatStyle.Flat
        btnClose.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        AddHandler btnClose.Click, Sub()
                                       confirmForm.DialogResult = DialogResult.OK
                                       confirmForm.Close()
                                   End Sub
        buttonPanel.Controls.Add(btnClose)

        confirmForm.ShowDialog()
        confirmForm.Dispose()
    End Sub

    Private Sub PrintBatchReceiptContent(bookingReferences As List(Of String), submittedBookings As List(Of BookingForm.BookingItem), totalAmount As Decimal, discountAmount As Decimal)
        Try
            Dim sb As New StringBuilder()

            ' Header
            sb.AppendLine("╔════════════════════════════════════════════════════════════════╗")
            sb.AppendLine("║                    LAKBAYPH TRAVEL AND TOURS                   ║")
            sb.AppendLine("║                      BATCH BOOKING RECEIPT                     ║")
            sb.AppendLine("╚════════════════════════════════════════════════════════════════╝")
            sb.AppendLine()
            sb.AppendLine($"Receipt Date: {DateTime.Now:MMMM dd, yyyy hh:mm tt}")
            sb.AppendLine($"Total Bookings: {submittedBookings.Count}")
            sb.AppendLine("================================================================")
            sb.AppendLine()

            ' Booking References
            sb.AppendLine("BOOKING REFERENCES")
            sb.AppendLine("─────────────────")
            For i As Integer = 0 To bookingReferences.Count - 1
                sb.AppendLine($"Booking {i + 1}: {bookingReferences(i)}")
            Next
            sb.AppendLine($"Batch Status: Pending Admin Approval")
            sb.AppendLine($"Submission Date: {DateTime.Now:MMM dd, yyyy HH:mm}")
            sb.AppendLine()

            ' Customer Information
            sb.AppendLine("CUSTOMER INFORMATION")
            sb.AppendLine("────────────────────")
            sb.AppendLine($"Name: {CurrentUserName}")
            sb.AppendLine($"Email: {CurrentUserEmail}")
            sb.AppendLine($"Phone: {CurrentUserPhone}")
            sb.AppendLine()

            ' Individual Booking Details
            sb.AppendLine("BOOKING DETAILS")
            sb.AppendLine("───────────────")
            For i As Integer = 0 To submittedBookings.Count - 1
                Dim booking = submittedBookings(i)
                sb.AppendLine($"BOOKING {i + 1} - {bookingReferences(i)}")
                sb.AppendLine($"Package: {booking.PackageName}")
                sb.AppendLine($"Package Type: {booking.PackageType}")
                sb.AppendLine($"Travel Date: {booking.BookingDate:MMM dd, yyyy}")
                sb.AppendLine($"Number of People: {booking.NumberOfPeople}")
                sb.AppendLine($"Add-ons: {booking.AddOns}")
                sb.AppendLine($"Amount: ₱{booking.TotalPrice:N2}")
                sb.AppendLine()
            Next

            ' Pricing Summary
            Dim originalTotal = submittedBookings.Sum(Function(b) b.TotalPrice)
            sb.AppendLine("PRICING SUMMARY")
            sb.AppendLine("───────────────")
            sb.AppendLine($"Original Total: ₱{originalTotal:N2}")

            If discountAmount > 0 AndAlso appliedPromoDetails IsNot Nothing Then
                sb.AppendLine($"Discount Applied: ₱{discountAmount:N2} ({appliedPromoDetails.PromoTypeName})")
                sb.AppendLine($"You Saved: ₱{discountAmount:N2}")
            Else
                sb.AppendLine("Discount Applied: None")
            End If

            sb.AppendLine($"FINAL TOTAL: ₱{totalAmount:N2}")
            sb.AppendLine()

            ' Important Notes
            sb.AppendLine("IMPORTANT NOTES")
            sb.AppendLine("───────────────")
            sb.AppendLine($"• All {submittedBookings.Count} bookings are currently PENDING admin approval")
            sb.AppendLine("• You will receive confirmation emails for each booking once approved")
            sb.AppendLine("• Please keep all booking reference numbers for future inquiries")
            sb.AppendLine("• Contact customer service for any questions or changes")

            If discountAmount > 0 AndAlso appliedPromoDetails IsNot Nothing Then
                sb.AppendLine($"• Your {appliedPromoDetails.PromoTypeName} discount has been applied to this batch")
            End If

            sb.AppendLine()
            sb.AppendLine("================================================================")
            sb.AppendLine("           Thank you for choosing LakbayPH Travel and Tours!")
            sb.AppendLine("================================================================")

            ' Save dialog
            Dim saveDialog As New SaveFileDialog()
            saveDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            saveDialog.DefaultExt = "txt"
            saveDialog.FileName = $"BatchBookingReceipt_{CurrentUserName.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.txt"

            If saveDialog.ShowDialog() = DialogResult.OK Then
                File.WriteAllText(saveDialog.FileName, sb.ToString())
                MessageBox.Show($"Receipt exported successfully to:{vbCrLf}{saveDialog.FileName}",
                          "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)

                Dim result As DialogResult = MessageBox.Show("Would you like to open the receipt now?",
                                                     "Open Receipt", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If result = DialogResult.Yes Then
                    Process.Start("notepad.exe", saveDialog.FileName)
                End If
            End If

        Catch ex As Exception
            MessageBox.Show($"Error exporting receipt: {ex.Message}", "Export Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Helper methods for creating confirmation sections and rows
    Private Function CreateConfirmationSection(parent As Panel, title As String, yPos As Integer) As Integer
        Dim sectionLabel As New Label()
        sectionLabel.Text = title
        sectionLabel.Location = New Point(20, yPos)
        sectionLabel.Size = New Size(600, 25)
        sectionLabel.Font = New Font("Segoe UI", 11, FontStyle.Bold)
        sectionLabel.ForeColor = Color.FromArgb(70, 130, 180)
        sectionLabel.BackColor = Color.FromArgb(240, 248, 255)
        sectionLabel.BorderStyle = BorderStyle.FixedSingle
        sectionLabel.TextAlign = ContentAlignment.MiddleLeft
        sectionLabel.Padding = New Padding(10, 0, 0, 0)
        parent.Controls.Add(sectionLabel)
        Return yPos + 35
    End Function

    Private Function CreateConfirmationRow(parent As Panel, label As String, value As String, yPos As Integer, Optional isBold As Boolean = False) As Integer
        Dim lblLabel As New Label()
        lblLabel.Text = label
        lblLabel.Location = New Point(30, yPos)
        lblLabel.Size = New Size(200, 20)
        lblLabel.Font = If(isBold, New Font("Segoe UI", 10, FontStyle.Bold), New Font("Segoe UI", 9))
        lblLabel.ForeColor = If(isBold, Color.FromArgb(34, 139, 34), Color.FromArgb(80, 80, 80))
        parent.Controls.Add(lblLabel)

        Dim lblValue As New Label()
        lblValue.Text = value
        lblValue.Location = New Point(250, yPos)
        lblValue.Size = New Size(350, 20)
        lblValue.Font = If(isBold, New Font("Segoe UI", 10, FontStyle.Bold), New Font("Segoe UI", 9))
        lblValue.ForeColor = If(isBold, Color.FromArgb(34, 139, 34), Color.Black)
        lblValue.TextAlign = ContentAlignment.MiddleRight
        parent.Controls.Add(lblValue)
        Return yPos + 25
    End Function

    ' Public method to load bookings from external sources
    Public Sub LoadBookings(bookings As List(Of BookingForm.BookingItem))
        ' This method can be called externally to load bookings
        BookingForm.BookingList.Clear()
        BookingForm.BookingList.AddRange(bookings)
        CreatePackageTypeTabs()
    End Sub
End Class
