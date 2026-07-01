Imports System.Data.SqlClient
Imports System.ComponentModel
Imports System.Text
Imports System.IO
Imports MySql.Data.MySqlClient
Public Class BookingForm
    Inherits Form

    ' Database connection string
    Dim connectionString As String = "Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;"

    Public sql As String
    Public dbcomm As MySqlCommand

    ' User Authentication Properties
    Public Property CurrentUserID As Integer
    Public Property CurrentUserName As String = ""
    Public Property CurrentUserEmail As String = ""
    Public Property CurrentUserPhone As String = ""
    Public Property DatabaseID As Integer

    ' Form controls
    Private WithEvents lblTitle As New Label()
    Private WithEvents cmbCategories As New ComboBox()
    Private WithEvents cmbPackages As New ComboBox()
    Private WithEvents pnlPackageDetails As New Panel()
    Private WithEvents lblPackageDetails As New Label()
    Private WithEvents dtpBookingDate As New DateTimePicker()
    Private WithEvents nudNumberOfPeople As New NumericUpDown()
    Private WithEvents pnlAddOns As New Panel()
    Private WithEvents scrlAddOns As New VScrollBar()
    Private WithEvents cmbPaymentMethod As New ComboBox()

    ' Promo discount controls
    Private WithEvents cmbPromoDiscount As New ComboBox()
    Private WithEvents lblOriginalAmount As New Label()
    Private WithEvents lblDiscountAmount As New Label()
    Private WithEvents lblDiscountPercent As New Label()

    Private WithEvents lblCurrentUser As New Label()
    Private WithEvents lblUserName As New Label()
    Private WithEvents lblUserEmail As New Label()
    Private WithEvents lblUserPhone As New Label()
    Private WithEvents lblTotalAmount As New Label()
    Private WithEvents btnCalculateTotal As New Button()
    Private WithEvents btnSubmitBooking As New Button()
    Private WithEvents btnClear As New Button()
    Private WithEvents btnRefreshPackages As New Button()

    ' Data structures
    Private tourPackage As New List(Of TourPackages)()
    Private allTourPackages As New List(Of TourPackages)()
    Private categories As New List(Of TourCategories)()
    Private addOns As New List(Of PackageAddOns)()
    Private addOnCheckBoxes As New List(Of CheckBox)()
    Private availablePromos As New List(Of PromoDiscounts)()

    Private currentTotal As Decimal = 0
    Private originalAmount As Decimal = 0
    Private discountAmount As Decimal = 0
    Private selectedPromoID As Integer = 0
    Private appliedPromoDetails As PromoDiscounts = Nothing
    Private CurrentUserFullName As String
    Private CurrentUserLastName As Object
    Private CurrentUserFirstName As Object
    Friend BookingSuccess As Boolean

    Public Property PreselectedPackage As Object

    ' Add to List Controls
    Private WithEvents btnAddToList As New Button()
    Public Shared BookingList As New List(Of BookingItem)()

    ' Package Add-on class
    Public Class PackageAddOns
        Public Property AddOnID As Integer
        Public Property packageID As Integer
        Public Property AddOnName As String
        Public Property Price As Decimal
        Public Property Unit As String
        Public Property IsActive As Boolean = True
    End Class

    ' Tour Package class
    Public Class TourPackages
        Public Property PackageID As Integer
        Public Property Title As String
        Public Property Location As String
        Public Property PackageType As String
        Public Property Price As Decimal
        Public Property Duration As Integer
        Public Property DurationNights As Integer
        Public Property TotalSlots As Integer
        Public Property AvailableSlots As Integer
        Public Property StartDate As DateTime
        Public Property EndDate As DateTime
        Public Property IsGroupPackage As Boolean
        Public Property GroupSize As Integer
        Public Property CategoryName As String
        Public Property CategoryID As Integer
        Public Property MaxSlots As Integer
        Public Property Status As String
    End Class

    ' Tour Category class
    Public Class TourCategories
        Public Property CategoryID As Integer
        Public Property CategoryName As String
        Public Property CategoryType As String
        Public Property IsActive As Boolean = True
    End Class

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

    ' Travel Package class (for preselected packages)
    Public Class TravelPackages
        Public Property ID As Integer
        Public Property Name As String
        Public Property Destination As String
        Public Property Price As Decimal
        Public Property Duration As String
        Public Property Description As String
        Public Property ImagePath As String
        Public Property PackageType As String = "Travel"
    End Class

    ' Freediving Package class (for preselected packages)
    Public Class FreedivingPackages
        Public Property ID As Integer
        Public Property Name As String
        Public Property Location As String
        Public Property Price As Decimal
        Public Property Duration As String
        Public Property Description As String
        Public Property ImagePath As String
        Public Property Level As String
        Public Property PackageType As String = "Freediving"
    End Class

    ' BookingItem class for booking list
    Public Class BookingItem
        Public Property PackageName As String
        Public Property PackageType As String
        Public Property NumberOfPeople As Integer
        Public Property AddOns As String
        Public Property TotalPrice As Decimal
        Public Property BookingDate As DateTime
        Public Property PackageID As Integer ' Add this for better identification
        Public Property UserID As Integer ' Add this for user-specific lists
        Public Property UniqueID As String ' Add this for duplicate detection
    End Class

    ' Update the constructor
    Public Sub New(UserID As Integer, Username As String, Email As String, Phone As String, Optional selectedPackage As Object = Nothing, Optional package As TravelPackage = Nothing)
        CurrentUserID = UserID
        CurrentUserName = Username
        CurrentUserEmail = Email
        CurrentUserPhone = Phone
        PreselectedPackage = selectedPackage

        InitializeComponents()
        LoadData()
        SetupForm()
        DisplayUserInfo()

        ' If a package was preselected, auto-select it
        If selectedPackage IsNot Nothing Then
            AutoSelectPackage(selectedPackage)
        End If
    End Sub

    ' Update the AutoSelectPackage method to handle both package types
    Private Sub AutoSelectPackage(selectedPackage As Object)
        Try
            ' First load all data if not already loaded
            If categories.Count = 0 Then
                LoadTourCategoriesFromDB()
            End If

            If allTourPackages.Count = 0 Then
                LoadTourPackagesFromDB()
            End If

            ' Determine package type and set search criteria
            Dim packageName As String = ""
            Dim packageLocation As String = ""
            Dim isFreediving As Boolean = False

            If TypeOf selectedPackage Is FreedivingPackage Then
                Dim freedivingPkg = CType(selectedPackage, FreedivingPackage)
                packageName = freedivingPkg.Name
                packageLocation = freedivingPkg.Location
                isFreediving = True
            ElseIf TypeOf selectedPackage Is TravelPackage Then
                Dim travelPkg = CType(selectedPackage, TravelPackage)
                packageName = travelPkg.Name
                packageLocation = travelPkg.Destination
            Else
                MessageBox.Show("Invalid package type provided", "Error",
                           MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            ' Find the matching package in our database by title and location
            Dim matchingPackage = allTourPackages.FirstOrDefault(Function(p)
                                                                     ' Match by title (case insensitive) and location
                                                                     Return p.Title.Equals(packageName, StringComparison.OrdinalIgnoreCase) AndAlso
                   GetPackageLocation(p.PackageID).Equals(packageLocation, StringComparison.OrdinalIgnoreCase) AndAlso
                   (Not isFreediving OrElse p.PackageType = "Freediving")
                                                                 End Function)

            If matchingPackage IsNot Nothing Then
                ' Find and select the category
                Dim matchingCategoryIndex = -1
                For i As Integer = 0 To categories.Count - 1
                    If categories(i).CategoryID = matchingPackage.CategoryID Then
                        matchingCategoryIndex = i
                        Exit For
                    End If
                Next

                If matchingCategoryIndex >= 0 Then
                    ' Select the category
                    cmbCategories.SelectedIndex = matchingCategoryIndex + 1 ' +1 because index 0 is "-- Select Category --"

                    ' Wait for the packages to load
                    Application.DoEvents()

                    ' Find and select the package in the combobox
                    For i As Integer = 0 To tourPackage.Count - 1
                        If tourPackage(i).PackageID = matchingPackage.PackageID Then
                            cmbPackages.SelectedIndex = i + 1 ' +1 because index 0 is "-- Select Package --"

                            ' Automatically calculate total for 1 person
                            nudNumberOfPeople.Value = 1
                            CalculateTotal()

                            Exit For
                        End If
                    Next
                End If
            Else
                MessageBox.Show($"The selected package '{packageName}' wasn't found in our current offerings. Please select manually.",
                           "Package Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            MessageBox.Show($"Error auto-selecting package: {ex.Message}", "Error",
                       MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub DisplayUserInfo()
        lblCurrentUser.Text = "Booking for:"
        lblUserName.Text = $"Name: {CurrentUserName}"
        lblUserEmail.Text = $"Email: {CurrentUserEmail}"
    End Sub

    Private Sub InitializeComponents()

        ' Form setup - Made larger to accommodate promo section
        Me.Text = "LakbayPH Travel and Tours - Booking System"
        Me.Size = New Size(1200, 950)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(240, 248, 255)
        Me.MinimumSize = New Size(1200, 950)

        ' Title
        lblTitle.Text = "LakbayPH Travel and Tours Booking"
        lblTitle.Font = New Font("Arial", 18, FontStyle.Bold)
        lblTitle.ForeColor = Color.FromArgb(25, 118, 210)
        lblTitle.Location = New Point(30, 20)
        lblTitle.Size = New Size(500, 35)

        ' Left Column - Package Selection
        Dim lblCategory As New Label()
        lblCategory.Text = "Select Category:"
        lblCategory.Font = New Font("Arial", 11, FontStyle.Bold)
        lblCategory.Location = New Point(30, 80)
        lblCategory.Size = New Size(150, 25)

        cmbCategories.Location = New Point(30, 110)
        cmbCategories.Size = New Size(280, 30)
        cmbCategories.DropDownStyle = ComboBoxStyle.DropDownList
        cmbCategories.Font = New Font("Arial", 10)

        ' Refresh button
        btnRefreshPackages.Text = "Refresh"
        btnRefreshPackages.Location = New Point(320, 110)
        btnRefreshPackages.Size = New Size(80, 30)
        btnRefreshPackages.BackColor = Color.FromArgb(156, 39, 176)
        btnRefreshPackages.ForeColor = Color.White
        btnRefreshPackages.FlatStyle = FlatStyle.Flat
        btnRefreshPackages.Font = New Font("Arial", 9, FontStyle.Bold)

        ' Package selection
        Dim lblPackage As New Label()
        lblPackage.Text = "Select Tour Package:"
        lblPackage.Font = New Font("Arial", 11, FontStyle.Bold)
        lblPackage.Location = New Point(30, 160)
        lblPackage.Size = New Size(180, 25)

        cmbPackages.Location = New Point(30, 190)
        cmbPackages.Size = New Size(450, 30)
        cmbPackages.DropDownStyle = ComboBoxStyle.DropDownList
        cmbPackages.Font = New Font("Arial", 10)

        ' Package details panel
        Dim lblPackageDetailsHeader As New Label()
        lblPackageDetailsHeader.Text = "Package Details:"
        lblPackageDetailsHeader.Font = New Font("Arial", 11, FontStyle.Bold)
        lblPackageDetailsHeader.Location = New Point(30, 240)
        lblPackageDetailsHeader.Size = New Size(150, 25)

        pnlPackageDetails.Location = New Point(30, 270)
        pnlPackageDetails.Size = New Size(450, 180)
        pnlPackageDetails.BorderStyle = BorderStyle.FixedSingle
        pnlPackageDetails.BackColor = Color.White
        pnlPackageDetails.AutoScroll = True

        lblPackageDetails.Location = New Point(10, 10)
        lblPackageDetails.Size = New Size(420, 160)
        lblPackageDetails.Font = New Font("Arial", 9)
        lblPackageDetails.ForeColor = Color.FromArgb(64, 64, 64)
        lblPackageDetails.Text = "Select a category first, then choose a package to view details..."
        pnlPackageDetails.Controls.Add(lblPackageDetails)

        ' Right Column - Add-ons
        Dim lblAddOns As New Label()
        lblAddOns.Text = "Optional Add-ons:"
        lblAddOns.Font = New Font("Arial", 11, FontStyle.Bold)
        lblAddOns.Location = New Point(520, 80)
        lblAddOns.Size = New Size(150, 25)

        ' Add-ons panel with individual checkboxes
        pnlAddOns.Location = New Point(520, 110)
        pnlAddOns.Size = New Size(300, 160)
        pnlAddOns.BorderStyle = BorderStyle.FixedSingle
        pnlAddOns.BackColor = Color.White
        pnlAddOns.AutoScroll = True

        ' Add default message
        Dim lblNoAddOns As New Label()
        lblNoAddOns.Text = "Select a package to view available add-ons"
        lblNoAddOns.Location = New Point(10, 10)
        lblNoAddOns.Size = New Size(280, 50)
        lblNoAddOns.Font = New Font("Arial", 9)
        lblNoAddOns.ForeColor = Color.Gray
        lblNoAddOns.TextAlign = ContentAlignment.MiddleCenter
        lblNoAddOns.Name = "lblNoAddOns"
        pnlAddOns.Controls.Add(lblNoAddOns)

        ' Promo Discount Section
        Dim lblPromoHeader As New Label()
        lblPromoHeader.Text = "Available Discounts:"
        lblPromoHeader.Font = New Font("Arial", 11, FontStyle.Bold)
        lblPromoHeader.ForeColor = Color.FromArgb(255, 87, 34)
        lblPromoHeader.Location = New Point(860, 80)
        lblPromoHeader.Size = New Size(170, 25)

        Dim lblPromoSelect As New Label()
        lblPromoSelect.Text = "Select Discount:"
        lblPromoSelect.Font = New Font("Arial", 10, FontStyle.Bold)
        lblPromoSelect.Location = New Point(860, 110)
        lblPromoSelect.Size = New Size(120, 25)

        cmbPromoDiscount.Location = New Point(860, 135)
        cmbPromoDiscount.Size = New Size(200, 30)
        cmbPromoDiscount.DropDownStyle = ComboBoxStyle.DropDownList
        cmbPromoDiscount.Font = New Font("Arial", 10)

        ' Pricing breakdown section
        Dim lblPricingHeader As New Label()
        lblPricingHeader.Text = "Price Breakdown:"
        lblPricingHeader.Font = New Font("Arial", 12, FontStyle.Bold)
        lblPricingHeader.ForeColor = Color.FromArgb(25, 118, 210)
        lblPricingHeader.Location = New Point(520, 290)
        lblPricingHeader.Size = New Size(150, 25)

        lblOriginalAmount.Text = "Original Amount: ₱0.00"
        lblOriginalAmount.Font = New Font("Arial", 11)
        lblOriginalAmount.ForeColor = Color.FromArgb(100, 100, 100)
        lblOriginalAmount.Location = New Point(520, 320)
        lblOriginalAmount.Size = New Size(250, 25)

        lblDiscountPercent.Text = ""
        lblDiscountPercent.Font = New Font("Arial", 10, FontStyle.Italic)
        lblDiscountPercent.ForeColor = Color.FromArgb(255, 87, 34)
        lblDiscountPercent.Location = New Point(520, 345)
        lblDiscountPercent.Size = New Size(250, 20)

        lblDiscountAmount.Text = "Discount: ₱0.00"
        lblDiscountAmount.Font = New Font("Arial", 11)
        lblDiscountAmount.ForeColor = Color.FromArgb(255, 87, 34)
        lblDiscountAmount.Location = New Point(520, 365)
        lblDiscountAmount.Size = New Size(250, 25)

        lblTotalAmount.Text = "Final Amount: ₱0.00"
        lblTotalAmount.Font = New Font("Arial", 14, FontStyle.Bold)
        lblTotalAmount.ForeColor = Color.FromArgb(76, 175, 80)
        lblTotalAmount.Location = New Point(520, 395)
        lblTotalAmount.Size = New Size(300, 30)

        ' Calculate Total button
        btnCalculateTotal.Text = "Calculate Total"
        btnCalculateTotal.Location = New Point(520, 435)
        btnCalculateTotal.Size = New Size(130, 40)
        btnCalculateTotal.BackColor = Color.FromArgb(33, 150, 243)
        btnCalculateTotal.ForeColor = Color.White
        btnCalculateTotal.FlatStyle = FlatStyle.Flat
        btnCalculateTotal.Font = New Font("Arial", 10, FontStyle.Bold)

        ' Submit Booking button
        btnSubmitBooking.Text = "Submit Booking"
        btnSubmitBooking.Location = New Point(660, 435)
        btnSubmitBooking.Size = New Size(130, 40)
        btnSubmitBooking.BackColor = Color.FromArgb(76, 175, 80)
        btnSubmitBooking.ForeColor = Color.White
        btnSubmitBooking.FlatStyle = FlatStyle.Flat
        btnSubmitBooking.Font = New Font("Arial", 10, FontStyle.Bold)

        ' Add to List button
        btnAddToList.Text = "Add to List"
        btnAddToList.Location = New Point(800, 435)
        btnAddToList.Size = New Size(130, 40)
        btnAddToList.BackColor = Color.FromArgb(255, 152, 0)
        btnAddToList.ForeColor = Color.White
        btnAddToList.FlatStyle = FlatStyle.Flat
        btnAddToList.Font = New Font("Arial", 10, FontStyle.Bold)

        ' Booking Information Row
        Dim lblBookingInfo As New Label()
        lblBookingInfo.Text = "Booking Information"
        lblBookingInfo.Font = New Font("Arial", 12, FontStyle.Bold)
        lblBookingInfo.ForeColor = Color.FromArgb(25, 118, 210)
        lblBookingInfo.Location = New Point(30, 490)
        lblBookingInfo.Size = New Size(200, 25)

        ' Booking date
        Dim lblBookingDate As New Label()
        lblBookingDate.Text = "Preferred Booking Date:"
        lblBookingDate.Font = New Font("Arial", 10, FontStyle.Bold)
        lblBookingDate.Location = New Point(30, 530)
        lblBookingDate.Size = New Size(180, 25)

        dtpBookingDate.Location = New Point(30, 560)
        dtpBookingDate.Size = New Size(200, 30)
        dtpBookingDate.Format = DateTimePickerFormat.Short
        dtpBookingDate.MinDate = DateTime.Today
        dtpBookingDate.Font = New Font("Arial", 10)

        ' Number of people
        Dim lblPeople As New Label()
        lblPeople.Text = "Number of People:"
        lblPeople.Font = New Font("Arial", 10, FontStyle.Bold)
        lblPeople.Location = New Point(260, 530)
        lblPeople.Size = New Size(150, 25)

        nudNumberOfPeople.Location = New Point(260, 560)
        nudNumberOfPeople.Size = New Size(100, 30)
        nudNumberOfPeople.Minimum = 1
        nudNumberOfPeople.Maximum = 50
        nudNumberOfPeople.Value = 1
        nudNumberOfPeople.Font = New Font("Arial", 10)

        ' Payment method
        Dim lblPayment As New Label()
        lblPayment.Text = "Payment Method:"
        lblPayment.Font = New Font("Arial", 10, FontStyle.Bold)
        lblPayment.Location = New Point(410, 530)
        lblPayment.Size = New Size(150, 25)

        cmbPaymentMethod.Location = New Point(410, 560)
        cmbPaymentMethod.Size = New Size(180, 30)
        cmbPaymentMethod.DropDownStyle = ComboBoxStyle.DropDownList
        cmbPaymentMethod.Items.Add("Pay on Trip")
        cmbPaymentMethod.SelectedIndex = 0
        cmbPaymentMethod.Font = New Font("Arial", 10)

        ' Customer information section (Display Only - No input fields)
        Dim lblCustomerInfo As New Label()
        lblCustomerInfo.Text = "Customer Information (Current User)"
        lblCustomerInfo.Font = New Font("Arial", 12, FontStyle.Bold)
        lblCustomerInfo.ForeColor = Color.FromArgb(25, 118, 210)
        lblCustomerInfo.Location = New Point(30, 610)
        lblCustomerInfo.Size = New Size(300, 25)

        lblCurrentUser.Text = "Booking for:"
        lblCurrentUser.Font = New Font("Arial", 10, FontStyle.Bold)
        lblCurrentUser.Location = New Point(30, 650)
        lblCurrentUser.Size = New Size(100, 25)

        lblUserName.Font = New Font("Arial", 10)
        lblUserName.Location = New Point(140, 650)
        lblUserName.Size = New Size(250, 25)

        lblUserEmail.Font = New Font("Arial", 10)
        lblUserEmail.Location = New Point(420, 650)
        lblUserEmail.Size = New Size(300, 25)

        lblUserPhone.Font = New Font("Arial", 10)
        lblUserPhone.Location = New Point(140, 680)
        lblUserPhone.Size = New Size(200, 25)

        ' Clear button
        btnClear.Text = "Clear Form"
        btnClear.Location = New Point(950, 840)
        btnClear.Size = New Size(130, 40)
        btnClear.BackColor = Color.FromArgb(244, 67, 54)
        btnClear.ForeColor = Color.White
        btnClear.FlatStyle = FlatStyle.Flat
        btnClear.Font = New Font("Arial", 10, FontStyle.Bold)

        AddHandler cmbCategories.SelectedIndexChanged, AddressOf cmbCategories_SelectedIndexChanged
        AddHandler cmbPackages.SelectedIndexChanged, AddressOf cmbPackages_SelectedIndexChanged
        AddHandler cmbPromoDiscount.SelectedIndexChanged, AddressOf cmbPromoDiscount_SelectedIndexChanged
        AddHandler nudNumberOfPeople.ValueChanged, AddressOf nudNumberOfPeople_ValueChanged
        AddHandler btnCalculateTotal.Click, AddressOf btnCalculateTotal_Click
        AddHandler btnRefreshPackages.Click, AddressOf btnRefreshPackages_Click
        AddHandler btnSubmitBooking.Click, AddressOf btnSubmitBooking_Click
        AddHandler btnClear.Click, AddressOf btnClear_Click
        AddHandler btnAddToList.Click, AddressOf btnAddToList_Click


        ' Add all controls to form
        Me.Controls.AddRange({lblTitle, lblCategory, cmbCategories, btnRefreshPackages,
                     lblPackage, cmbPackages, lblPackageDetailsHeader, pnlPackageDetails,
                     lblBookingInfo, lblBookingDate, dtpBookingDate, lblPeople, nudNumberOfPeople,
                     lblAddOns, pnlAddOns, lblPayment, cmbPaymentMethod,
                     lblPromoHeader, lblPromoSelect, cmbPromoDiscount,
                     lblPricingHeader, lblOriginalAmount, lblDiscountPercent, lblDiscountAmount,
                     lblCustomerInfo, lblCurrentUser, lblUserName, lblUserEmail, lblUserPhone,
                     lblTotalAmount, btnCalculateTotal, btnSubmitBooking, btnAddToList, btnClear})
    End Sub

    Private Sub SetupForm()
        LoadTourCategories()
        LoadUserPromoDiscounts()
    End Sub

    Private Sub LoadData()
        LoadTourCategoriesFromDB()
        LoadTourPackagesFromDB()
        LoadUserPromoDiscounts()
        CalculateTotal()
    End Sub

    ' Load user's approved promo discounts
    Private Sub LoadUserPromoDiscounts()
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim query As String = "SELECT pd.PromoID, pd.PromoTypeID, pt.PromoTypeName, pt.DiscountPercentage, pd.DocumentNumber " &
                         "FROM PromoDiscounts pd " &
                         "INNER JOIN PromoTypes pt ON pd.PromoTypeID = pt.PromoTypeID " &
                         "WHERE pd.UserID = @UserID AND pd.Status = 'Approved' AND pt.IsActive = 1 " &
                         "AND (pd.ExpiryDate IS NULL OR pd.ExpiryDate > CURDATE())"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@UserID", CurrentUserID)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        availablePromos.Clear()
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
            End Using

            ' Populate promo combo box
            cmbPromoDiscount.Items.Clear()
            cmbPromoDiscount.Items.Add("-- No Discount --")

            ' Add all available promo types, not just user-specific ones
            Dim allPromoTypes As New List(Of PromoDiscounts)()
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim promoQuery As String = "SELECT PromoTypeID, PromoTypeName, DiscountPercentage FROM PromoTypes WHERE IsActive = 1"
                Using cmd As New MySqlCommand(promoQuery, conn)
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

            ' Add both user-specific promos and all available promo types
            For Each promoType In allPromoTypes
                Dim isUserHasPromo = availablePromos.Any(Function(p) p.PromoTypeID = promoType.PromoTypeID)
                Dim displayText = $"{promoType.PromoTypeName} ({promoType.DiscountPercentage}% off)"

                If isUserHasPromo Then
                    ' Add with a checkmark or other indicator
                    cmbPromoDiscount.Items.Add($"✓ {displayText}")
                Else
                    ' Add as unavailable (grayed out)
                    cmbPromoDiscount.Items.Add(displayText)
                End If
            Next

            cmbPromoDiscount.SelectedIndex = 0

        Catch ex As Exception
            MessageBox.Show("Error loading promo discounts: " & ex.Message, "Database Error",
             MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LoadTourCategoriesFromDB()
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim query As String = "SELECT CategoryID, CategoryName, CategoryType FROM TourCategories " &
                           "WHERE IsActive = 1 ORDER BY CategoryName"
                Using cmd As New MySqlCommand(query, conn)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        categories.Clear()
                        While reader.Read()
                            Dim category As New TourCategories() With {
                       .CategoryID = reader("CategoryID"),
                       .CategoryName = reader("CategoryName").ToString(),
                       .CategoryType = reader("CategoryType").ToString()
                   }
                            categories.Add(category)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading categories: " & ex.Message, "Database Error",
                 MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LoadTourPackagesFromDB()
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                ' Use dynamic calculation for available slots
                Dim query As String = "SELECT " &
                   "tp.PackageID, tp.Title, tp.PackageType, tp.Price, tp.Duration, tp.DurationNights, " &
                   "tp.MaxSlots, tp.StartDate, tp.EndDate, tp.IsGroupPackage, tp.GroupSize, tp.Status, " &
                   "tc.CategoryName, tc.CategoryID, " &
                   "COALESCE(pl.Location, 'Location TBD') as Location, " &
                   "COALESCE(booked.BookedSlots, 0) as BookedSlots, " &
                   "(tp.MaxSlots - COALESCE(booked.BookedSlots, 0)) as AvailableSlots " &
                   "FROM TourPackages tp " &
                   "INNER JOIN TourCategories tc ON tp.CategoryID = tc.CategoryID " &
                   "LEFT JOIN PackageLocations pl ON tp.PackageID = pl.PackageID " &
                   "LEFT JOIN ( " &
                   "    SELECT PackageID, SUM(NumberOfPeople) as BookedSlots " &
                   "    FROM Bookings " &
                   "    WHERE Status IN ('Pending', 'Confirmed') " &
                   "    GROUP BY PackageID " &
                   ") booked ON tp.PackageID = booked.PackageID " &
                   "WHERE tp.IsActive = 1 AND tp.Status = 'active' " &
                   "ORDER BY tp.PackageType, tp.Title"
                Using cmd As New MySqlCommand(query, conn)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        allTourPackages.Clear()
                        While reader.Read()
                            Dim package As New TourPackages() With {
                       .PackageID = reader("PackageID"),
                       .Title = reader("Title").ToString(),
                       .Location = reader("Location").ToString(),
                       .PackageType = reader("PackageType").ToString(),
                       .Price = Convert.ToDecimal(reader("Price")),
                       .Duration = Convert.ToInt32(reader("Duration")),
                       .DurationNights = Convert.ToInt32(reader("DurationNights")),
                       .MaxSlots = Convert.ToInt32(reader("MaxSlots")),
                       .AvailableSlots = Convert.ToInt32(reader("AvailableSlots")),
                       .StartDate = Convert.ToDateTime(reader("StartDate")),
                       .EndDate = Convert.ToDateTime(reader("EndDate")),
                       .IsGroupPackage = Convert.ToBoolean(reader("IsGroupPackage")),
                       .GroupSize = Convert.ToInt32(reader("GroupSize")),
                       .CategoryName = reader("CategoryName").ToString(),
                       .CategoryID = Convert.ToInt32(reader("CategoryID")),
                       .Status = reader("Status").ToString()
                   }
                            allTourPackages.Add(package)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading tour packages: " & ex.Message, "Database Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LoadTourCategories()
        cmbCategories.Items.Clear()
        cmbCategories.Items.Add("-- Select Category --")

        For Each category In categories
            cmbCategories.Items.Add($"{category.CategoryName} ({category.CategoryType})")
        Next

        If cmbCategories.Items.Count > 0 Then
            cmbCategories.SelectedIndex = 0
        End If
    End Sub

    Private Sub LoadTourPackages()
        cmbPackages.Items.Clear()
        tourPackage.Clear()

        If cmbCategories.SelectedIndex <= 0 Then
            cmbPackages.Items.Add("-- Select Category First --")
            cmbPackages.SelectedIndex = 0
            cmbPackages.Enabled = False
            lblPackageDetails.Text = "Please select a category first to view available packages."
            ClearAddOns()
            Return
        Else
            cmbPackages.Enabled = True
            cmbPackages.Items.Add("-- Select Package --")

            Dim selectedCategory = categories(cmbCategories.SelectedIndex - 1)
            For Each package In allTourPackages
                If package.CategoryID = selectedCategory.CategoryID Then
                    tourPackage.Add(package)
                End If
            Next

            For Each package In tourPackage
                cmbPackages.Items.Add($"{package.Title} - {package.PackageType} (₱{package.Price:N2})")
            Next

            If cmbPackages.Items.Count > 1 Then
                cmbPackages.SelectedIndex = 0
            Else
                cmbPackages.Items.Add("No packages available")
                cmbPackages.SelectedIndex = 1
                lblPackageDetails.Text = "No packages available for the selected category." & vbCrLf & vbCrLf &
                                       "Please try selecting a different category or refresh the data."
            End If
        End If
    End Sub

    Private Sub LoadAddOns(packageID As Integer)
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim query As String = "SELECT * FROM PackageAddOns WHERE PackageID = @PackageID AND IsActive = 1 ORDER BY AddOnName"
                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@PackageID", packageID)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        addOns.Clear()
                        While reader.Read()
                            Dim addOn As New PackageAddOns() With {
                       .AddOnID = reader("AddOnID"),
                       .packageID = reader("PackageID"),
                       .AddOnName = reader("AddOnName").ToString(),
                       .Price = Convert.ToDecimal(reader("Price")),
                       .Unit = If(IsDBNull(reader("Unit")), "", reader("Unit").ToString())
                   }
                            addOns.Add(addOn)
                        End While
                        CreateAddOnCheckboxes()
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading add-ons: " & ex.Message, "Database Error",
                 MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub CreateAddOnCheckboxes()
        ClearAddOns()

        If addOns.Count = 0 Then
            Dim lblNoAddOns As New Label()
            lblNoAddOns.Text = "No add-ons available for this package"
            lblNoAddOns.Location = New Point(10, 10)
            lblNoAddOns.Size = New Size(280, 30)
            lblNoAddOns.Font = New Font("Arial", 9)
            lblNoAddOns.ForeColor = Color.Gray
            lblNoAddOns.TextAlign = ContentAlignment.MiddleCenter
            lblNoAddOns.Name = "lblNoAddOns"
            pnlAddOns.Controls.Add(lblNoAddOns)
        Else
            Dim yPosition As Integer = 10

            For i As Integer = 0 To addOns.Count - 1
                Dim addOn = addOns(i)
                Dim chkAddOn As New CheckBox()

                chkAddOn.Text = $"{addOn.AddOnName} - ₱{addOn.Price:N2} {addOn.Unit}"
                chkAddOn.Location = New Point(10, yPosition)
                chkAddOn.Size = New Size(270, 25)
                chkAddOn.Font = New Font("Arial", 9)
                chkAddOn.ForeColor = Color.FromArgb(64, 64, 64)
                chkAddOn.Tag = i
                chkAddOn.Name = $"chkAddOn_{addOn.AddOnID}"

                AddHandler chkAddOn.CheckedChanged, AddressOf AddOnCheckBox_CheckedChanged

                addOnCheckBoxes.Add(chkAddOn)
                pnlAddOns.Controls.Add(chkAddOn)

                yPosition += 30
            Next

            If addOns.Count > 1 Then
                yPosition += 10

                Dim btnSelectAll As New Button()
                btnSelectAll.Text = "Select All"
                btnSelectAll.Location = New Point(10, yPosition)
                btnSelectAll.Size = New Size(80, 25)
                btnSelectAll.Font = New Font("Arial", 8)
                btnSelectAll.BackColor = Color.FromArgb(33, 150, 243)
                btnSelectAll.ForeColor = Color.White
                btnSelectAll.FlatStyle = FlatStyle.Flat
                btnSelectAll.Name = "btnSelectAll"
                AddHandler btnSelectAll.Click, AddressOf BtnSelectAll_Click
                pnlAddOns.Controls.Add(btnSelectAll)

                Dim btnClearAll As New Button()
                btnClearAll.Text = "Clear All"
                btnClearAll.Location = New Point(100, yPosition)
                btnClearAll.Size = New Size(80, 25)
                btnClearAll.Font = New Font("Arial", 8)
                btnClearAll.BackColor = Color.FromArgb(244, 67, 54)
                btnClearAll.ForeColor = Color.White
                btnClearAll.FlatStyle = FlatStyle.Flat
                btnClearAll.Name = "btnClearAll"
                AddHandler btnClearAll.Click, AddressOf BtnClearAll_Click
                pnlAddOns.Controls.Add(btnClearAll)
            End If
        End If
    End Sub

    Private Sub ClearAddOns()
        Dim controlsToRemove As New List(Of Control)

        For Each ctrl As Control In pnlAddOns.Controls
            If ctrl.Name.StartsWith("chkAddOn_") OrElse
               ctrl.Name = "btnSelectAll" OrElse
               ctrl.Name = "btnClearAll" OrElse
               ctrl.Name = "lblNoAddOns" Then
                controlsToRemove.Add(ctrl)
            End If
        Next

        For Each ctrl In controlsToRemove
            pnlAddOns.Controls.Remove(ctrl)
            ctrl.Dispose()
        Next

        addOnCheckBoxes.Clear()
    End Sub

    Private Sub BtnSelectAll_Click(sender As Object, e As EventArgs)
        For Each chk As CheckBox In addOnCheckBoxes
            chk.Checked = True
        Next
    End Sub

    Private Sub BtnClearAll_Click(sender As Object, e As EventArgs)
        For Each chk As CheckBox In addOnCheckBoxes
            chk.Checked = False
        Next
    End Sub

    Private Sub AddOnCheckBox_CheckedChanged(sender As Object, e As EventArgs)
        CalculateTotal()
    End Sub

    Private Sub cmbCategories_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbCategories.SelectedIndexChanged
        LoadTourPackages()
        ClearAddOns()
        CalculateTotal()
    End Sub

    Private Sub cmbPromoDiscount_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbPromoDiscount.SelectedIndexChanged
        CalculateTotal()
    End Sub

    Private Sub btnRefreshPackages_Click(sender As Object, e As EventArgs) Handles btnRefreshPackages.Click
        LoadData()
        LoadTourCategories()
        MessageBox.Show("Data refreshed successfully!", "Refresh Complete",
                       MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub cmbPackages_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbPackages.SelectedIndexChanged
        If cmbPackages.SelectedIndex > 0 And cmbPackages.SelectedIndex <= tourPackage.Count Then
            Dim selectedPackage = tourPackage(cmbPackages.SelectedIndex - 1)
            DisplayPackageDetails(selectedPackage)
            LoadAddOns(selectedPackage.PackageID)
            CalculateTotal() ' Make sure this is called here
        Else
            DisplayEmptyPackageDetails()
            ClearAddOns()
            ' Reset pricing display when no package selected
            lblOriginalAmount.Text = "Original Amount: ₱0.00"
            lblDiscountAmount.Text = "Discount: ₱0.00"
            lblDiscountPercent.Text = ""
            lblTotalAmount.Text = "Final Amount: ₱0.00"
            originalAmount = 0
            discountAmount = 0
            currentTotal = 0
        End If
    End Sub

    Private Sub DisplayPackageDetails(package As TourPackages)
        Dim detailsText As String = BuildPackageDetailsText(package)
        lblPackageDetails.Text = detailsText
    End Sub

    Private Function BuildPackageDetailsText(package As TourPackages) As String
        Dim detailsText As String = ""
        detailsText &= $"📍 DESTINATION: {package.Location}" & vbCrLf & vbCrLf
        detailsText &= $"🏷️ CATEGORY: {package.CategoryName}" & vbCrLf
        detailsText &= $"📦 PACKAGE TYPE: {package.PackageType}" & vbCrLf & vbCrLf
        detailsText &= $"⏱️ DURATION: {package.Duration} days, {package.DurationNights} nights" & vbCrLf
        detailsText &= $"💰 PRICE: ₱{package.Price:N2} per person" & vbCrLf
        detailsText &= $"👥 AVAILABLE SLOTS: {package.AvailableSlots} out of {package.TotalSlots}" & vbCrLf & vbCrLf
        If package.IsGroupPackage Then
            detailsText &= $"👨‍👩‍👧‍👦 GROUP PACKAGE (Recommended group size: {package.GroupSize} people)" & vbCrLf
        End If
        Return detailsText
    End Function

    Private Sub DisplayEmptyPackageDetails()
        lblPackageDetails.Text = If(cmbCategories.SelectedIndex <= 0,
                              "Please select a category first to view available packages.",
                              "Please select a package to view details...")
    End Sub

    Private Sub nudNumberOfPeople_ValueChanged(sender As Object, e As EventArgs) Handles nudNumberOfPeople.ValueChanged
        CalculateTotal()
    End Sub

    Private Sub btnCalculateTotal_Click(sender As Object, e As EventArgs) Handles btnCalculateTotal.Click
        CalculateTotal()
    End Sub

    ' Update the CalculateTotal method to properly handle discounts
    Private Sub CalculateTotal()
        If cmbPackages.SelectedIndex <= 0 Or cmbPackages.SelectedIndex > tourPackage.Count Then
            lblOriginalAmount.Text = "Original Amount: ₱0.00"
            lblDiscountAmount.Text = "Discount: ₱0.00"
            lblDiscountPercent.Text = ""
            lblTotalAmount.Text = "Final Amount: ₱0.00"
            originalAmount = 0
            discountAmount = 0
            currentTotal = 0
            Return
        End If

        Dim selectedPackage = tourPackage(cmbPackages.SelectedIndex - 1)
        Dim numberOfPeople As Integer = CInt(nudNumberOfPeople.Value)

        ' Calculate base package amount
        originalAmount = selectedPackage.Price * numberOfPeople

        ' Calculate add-ons total
        For Each chkBox As CheckBox In addOnCheckBoxes
            If chkBox.Checked Then
                Dim addOnIndex As Integer = CInt(chkBox.Tag)
                originalAmount += addOns(addOnIndex).Price * numberOfPeople
            End If
        Next

        ' Reset discount amount before applying new discount
        discountAmount = 0

        ' Apply discount if any
        ApplyPromoDiscount()

        ' Calculate final total
        currentTotal = originalAmount - discountAmount

        ' Update the display labels - THIS WAS MISSING!
        UpdateTotalLabels()
    End Sub

    Private Sub ApplyPromoDiscount()
        discountAmount = 0 ' Reset discount
        appliedPromoDetails = Nothing
        selectedPromoID = 0

        If cmbPromoDiscount.SelectedIndex > 0 Then
            Dim selectedPromoText = cmbPromoDiscount.SelectedItem.ToString()

            ' Extract the promo type name (remove checkmark if present and percentage info)
            Dim promoTypeName As String

            If selectedPromoText.StartsWith("✓") Then
                promoTypeName = selectedPromoText.Substring(2).Split("(")(0).Trim()
            Else
                promoTypeName = selectedPromoText.Split("(")(0).Trim()
            End If

            Try
                Using conn As New MySqlConnection(connectionString)
                    conn.Open()
                    Dim discountQuery As String = "SELECT pt.PromoTypeID, pt.PromoTypeName, pt.DiscountPercentage, " &
                                    "pd.PromoID, pd.Status " &
                                    "FROM PromoTypes pt " &
                                    "LEFT JOIN PromoDiscounts pd ON pt.PromoTypeID = pd.PromoTypeID " &
                                    "AND pd.UserID = @UserID AND pd.Status = 'Approved' " &
                                    "WHERE pt.PromoTypeName = @PromoTypeName AND pt.IsActive = 1"

                    Using cmd As New MySqlCommand(discountQuery, conn)
                        cmd.Parameters.AddWithValue("@PromoTypeName", promoTypeName)
                        cmd.Parameters.AddWithValue("@UserID", CurrentUserID)

                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            If reader.Read() Then
                                Dim discountPercentage = Convert.ToDecimal(reader("DiscountPercentage"))
                                discountAmount = Math.Round(originalAmount * (discountPercentage / 100), 2)

                                appliedPromoDetails = New PromoDiscounts() With {
                            .PromoTypeID = Convert.ToInt32(reader("PromoTypeID")),
                            .PromoTypeName = reader("PromoTypeName").ToString(),
                            .DiscountPercentage = discountPercentage,
                            .PromoID = If(IsDBNull(reader("PromoID")), 0, Convert.ToInt32(reader("PromoID"))),
                            .Status = If(IsDBNull(reader("Status")), "", reader("Status").ToString())
                        }
                                selectedPromoID = appliedPromoDetails.PromoID
                                lblDiscountPercent.Text = $"({appliedPromoDetails.PromoTypeName} - {appliedPromoDetails.DiscountPercentage}% discount applied)"
                            End If
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                ' Log error but continue
                Console.WriteLine("Error applying discount: " & ex.Message)
                lblDiscountPercent.Text = ""
            End Try
        Else
            lblDiscountPercent.Text = ""
        End If
    End Sub

    ' Update display labels with proper formatting
    Private Sub UpdateTotalLabels()
        lblOriginalAmount.Text = $"Original Amount: ₱{originalAmount:N2}"

        If discountAmount > 0 Then
            lblDiscountAmount.Text = $"Discount: -₱{discountAmount:N2}"
            lblDiscountAmount.ForeColor = Color.FromArgb(255, 87, 34)
            lblTotalAmount.Text = $"Final Amount: ₱{currentTotal:N2}"
            lblTotalAmount.ForeColor = Color.FromArgb(76, 175, 80)
        Else
            lblDiscountAmount.Text = "Discount: ₱0.00"
            lblDiscountAmount.ForeColor = Color.FromArgb(100, 100, 100)
            lblTotalAmount.Text = $"Final Amount: ₱{currentTotal:N2}"
            lblTotalAmount.ForeColor = Color.FromArgb(25, 118, 210)
        End If
    End Sub

    Private Sub btnSubmitBooking_Click(sender As Object, e As EventArgs) Handles btnSubmitBooking.Click
        If ValidateForm() Then
            ShowBookingConfirmationPreview()
        End If
    End Sub

    Private Function ValidateForm() As Boolean
        ' Check if user is logged in
        If CurrentUserID <= 0 Then
            MessageBox.Show("No user is logged in. Please log in first to make a booking.", "Authentication Required",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        If cmbCategories.SelectedIndex <= 0 Then
            MessageBox.Show("Please select a category.", "Validation Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        If cmbPackages.SelectedIndex <= 0 Or cmbPackages.SelectedIndex > tourPackage.Count Then
            MessageBox.Show("Please select a tour package.", "Validation Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        If dtpBookingDate.Value < DateTime.Today Then
            MessageBox.Show("Booking date cannot be in the past.", "Validation Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning)
            dtpBookingDate.Focus()
            Return False
        End If

        If nudNumberOfPeople.Value < 1 Then
            MessageBox.Show("Number of people must be at least 1.", "Validation Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning)
            nudNumberOfPeople.Focus()
            Return False
        End If

        ' Check if selected package has enough available slots
        Dim selectedPackage = tourPackage(cmbPackages.SelectedIndex - 1)
        If nudNumberOfPeople.Value > selectedPackage.AvailableSlots Then
            MessageBox.Show($"Only {selectedPackage.AvailableSlots} slots are available for this package. Please adjust the number of people.",
                      "Insufficient Slots", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            nudNumberOfPeople.Focus()
            Return False
        End If

        If cmbPaymentMethod.SelectedIndex < 0 Then
            MessageBox.Show("Please select a payment method.", "Validation Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning)
            cmbPaymentMethod.Focus()
            Return False
        End If

        Return True
    End Function

    Private Sub ShowBookingConfirmationPreview()
        Dim selectedPackage = tourPackage(cmbPackages.SelectedIndex - 1)
        Dim packageLocation = GetPackageLocation(selectedPackage.PackageID)

        ' Create confirmation preview form
        Dim previewForm As New Form()
        previewForm.Text = "Confirm Your Booking"
        previewForm.Size = New Size(700, 800)
        previewForm.StartPosition = FormStartPosition.CenterParent
        previewForm.FormBorderStyle = FormBorderStyle.FixedDialog
        previewForm.MaximizeBox = False
        previewForm.MinimizeBox = False
        previewForm.BackColor = Color.White

        ' Header Panel
        Dim headerPanel As New Panel()
        headerPanel.Location = New Point(0, 0)
        headerPanel.Size = New Size(650, 80)
        headerPanel.BackColor = Color.FromArgb(25, 118, 210)
        previewForm.Controls.Add(headerPanel)

        Dim headerLabel As New Label()
        headerLabel.Text = "Confirm Your Booking Details"
        headerLabel.Location = New Point(30, 25)
        headerLabel.Size = New Size(400, 30)
        headerLabel.Font = New Font("Segoe UI", 16, FontStyle.Bold)
        headerLabel.ForeColor = Color.White
        headerPanel.Controls.Add(headerLabel)

        ' Content Panel
        Dim contentPanel As New Panel()
        contentPanel.Location = New Point(30, 100)
        contentPanel.Size = New Size(590, 580)
        contentPanel.BackColor = Color.FromArgb(248, 249, 250)
        contentPanel.BorderStyle = BorderStyle.FixedSingle
        contentPanel.AutoScroll = True
        previewForm.Controls.Add(contentPanel)

        Dim yPos As Integer = 20

        ' Package Details
        CreateConfirmationSection(contentPanel, "PACKAGE DETAILS", yPos)
        yPos += 40
        CreateConfirmationRow(contentPanel, "Package Name:", selectedPackage.Title, yPos)
        yPos += 25
        CreateConfirmationRow(contentPanel, "Package Price:", $"₱{selectedPackage.Price:N2} per person", yPos)
        yPos += 25
        CreateConfirmationRow(contentPanel, "Destination:", packageLocation, yPos)
        yPos += 25
        CreateConfirmationRow(contentPanel, "Travel Date:", dtpBookingDate.Value.ToString("MMM dd, yyyy"), yPos)
        yPos += 25
        CreateConfirmationRow(contentPanel, "Number of People:", nudNumberOfPeople.Value.ToString(), yPos)
        yPos += 35

        ' Add-ons section
        Dim selectedAddOns As New List(Of String)()
        Dim addOnTotal As Decimal = 0

        For Each chkBox As CheckBox In addOnCheckBoxes
            If chkBox.Checked Then
                Dim addOnIndex As Integer = CInt(chkBox.Tag)
                Dim addOnCost = addOns(addOnIndex).Price * nudNumberOfPeople.Value
                selectedAddOns.Add($"{addOns(addOnIndex).AddOnName} - ₱{addOns(addOnIndex).Price:N2}")
                addOnTotal += addOnCost
            End If
        Next

        If selectedAddOns.Count > 0 Then
            CreateConfirmationSection(contentPanel, "SELECTED ADD-ONS", yPos)
            yPos += 35
            For Each addOnText In selectedAddOns
                CreateConfirmationRow(contentPanel, "•", addOnText, yPos)
                yPos += 22
            Next
            yPos += 15
        Else
            CreateConfirmationSection(contentPanel, "ADD-ONS", yPos)
            yPos += 35
            CreateConfirmationRow(contentPanel, "", "No add-ons selected", yPos)
            yPos += 35
        End If

        ' Pricing Section
        CreateConfirmationSection(contentPanel, "PRICE BREAKDOWN", yPos)
        yPos += 40

        Dim packageCost = selectedPackage.Price * nudNumberOfPeople.Value
        CreateConfirmationRow(contentPanel, "Package Subtotal:", $"₱{packageCost:N2}", yPos)
        yPos += 25

        If addOnTotal > 0 Then
            CreateConfirmationRow(contentPanel, "Add-ons Total:", $"₱{addOnTotal:N2}", yPos)
            yPos += 25
        End If

        CreateConfirmationRow(contentPanel, "Subtotal:", $"₱{originalAmount:N2}", yPos, True)
        yPos += 30

        If discountAmount > 0 AndAlso appliedPromoDetails IsNot Nothing Then
            CreateConfirmationRow(contentPanel, "Discount:", $"-₱{discountAmount:N2} ({appliedPromoDetails.PromoTypeName})", yPos)
            yPos += 25
            CreateConfirmationRow(contentPanel, "FINAL TOTAL:", $"₱{currentTotal:N2}", yPos, True)
        Else
            CreateConfirmationRow(contentPanel, "FINAL TOTAL:", $"₱{currentTotal:N2}", yPos, True)
        End If

        ' Buttons Panel
        Dim buttonPanel As New Panel()
        buttonPanel.Location = New Point(0, 700)
        buttonPanel.Size = New Size(650, 80)
        buttonPanel.BackColor = Color.FromArgb(248, 249, 250)
        previewForm.Controls.Add(buttonPanel)

        ' Cancel Button
        Dim btnCancel As New Button()
        btnCancel.Text = "Cancel"
        btnCancel.Location = New Point(200, 20)
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
        btnConfirm.Text = "Confirm Booking"
        btnConfirm.Location = New Point(330, 20)
        btnConfirm.Size = New Size(120, 35)
        btnConfirm.BackColor = Color.FromArgb(76, 175, 80)
        btnConfirm.ForeColor = Color.White
        btnConfirm.FlatStyle = FlatStyle.Flat
        btnConfirm.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        AddHandler btnConfirm.Click, Sub()
                                         previewForm.Close()
                                         ProcessBookingSubmission()
                                     End Sub
        buttonPanel.Controls.Add(btnConfirm)

        previewForm.ShowDialog()
        previewForm.Dispose()
    End Sub

    Private Sub ProcessBookingSubmission()
        btnSubmitBooking.Enabled = False
        btnSubmitBooking.Text = "Processing..."

        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Using transaction As MySqlTransaction = conn.BeginTransaction()
                    Try
                        Dim selectedPackage = tourPackage(cmbPackages.SelectedIndex - 1)
                        Dim userSelectedTravelDate As DateTime = dtpBookingDate.Value
                        Dim calculatedEndDate As DateTime = userSelectedTravelDate.AddDays(selectedPackage.Duration - 1)
                        Dim nextId As Long = GetNextBookingId(conn, transaction)
                        Dim bookingReference As String = $"LKB-{DateTime.Now.Year}-{nextId.ToString().PadLeft(6, "0"c)}"

                        Dim bookingID As Long = InsertMainBookingWithPromo(conn, transaction, selectedPackage, userSelectedTravelDate, calculatedEndDate, bookingReference)

                        If bookingID <= 0 Then
                            Throw New Exception("Failed to create booking record")
                        End If

                        InsertBookingPayments(conn, transaction, bookingID)
                        InsertBookingAddOns(conn, transaction, bookingID)
                        InsertBookingHistory(conn, transaction, bookingID, bookingReference)
                        UpdatePackageSlots(conn, transaction, selectedPackage.PackageID)

                        transaction.Commit()

                        ShowBookingConfirmation(bookingReference, selectedPackage, userSelectedTravelDate, calculatedEndDate,
                                  CInt(nudNumberOfPeople.Value), currentTotal, cmbPaymentMethod.SelectedItem.ToString())

                    Catch ex As Exception
                        Try
                            transaction.Rollback()
                        Catch rollbackEx As Exception
                            Console.WriteLine("Rollback error: " & rollbackEx.Message)
                        End Try
                        MessageBox.Show("Error submitting booking: " & ex.Message, "Booking Error",
                           MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End Try
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error connecting to database: " & ex.Message, "Database Error",
               MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            btnSubmitBooking.Enabled = True
            btnSubmitBooking.Text = "Submit Booking"
        End Try
    End Sub

    ' Add this method to build the receipt content
    Private Function BuildReceiptContent() As String
        Try
            Dim sb As New StringBuilder()

            ' Header
            sb.AppendLine("╔════════════════════════════════════════════════════════════════╗")
            sb.AppendLine("║                    LAKBAYPH TRAVEL AND TOURS                   ║")
            sb.AppendLine("║                         BOOKING RECEIPT                        ║")
            sb.AppendLine("╚════════════════════════════════════════════════════════════════╝")
            sb.AppendLine()
            sb.AppendLine($"Receipt Date: {DateTime.Now:MMMM dd, yyyy hh:mm tt}")
            sb.AppendLine("================================================================")
            sb.AppendLine()

            ' Get selected package info
            If cmbPackages.SelectedIndex > 0 AndAlso cmbPackages.SelectedIndex <= tourPackage.Count Then
                Dim selectedPackage = tourPackage(cmbPackages.SelectedIndex - 1)

                ' Calculate amounts dynamically
                Dim numberOfPeople As Integer = CInt(nudNumberOfPeople.Value)
                Dim baseAmount As Decimal = selectedPackage.Price * numberOfPeople
                Dim addOnTotal As Decimal = 0

                ' Calculate add-ons total
                For Each chkBox As CheckBox In addOnCheckBoxes
                    If chkBox.Checked Then
                        Dim addOnIndex As Integer = CInt(chkBox.Tag)
                        addOnTotal += addOns(addOnIndex).Price * numberOfPeople
                    End If
                Next

                Dim subtotal As Decimal = baseAmount + addOnTotal
                Dim finalAmount As Decimal = currentTotal ' This is already calculated
                Dim discountApplied As Decimal = subtotal - finalAmount

                Dim travelDate = dtpBookingDate.Value
                Dim endDate = travelDate.AddDays(selectedPackage.Duration - 1)

                ' Booking Reference
                Dim bookingRef = $"LKB-{DateTime.Now.Year}-{DateTime.Now.Ticks.ToString().Substring(0, 6)}"
                sb.AppendLine("BOOKING REFERENCE INFORMATION")
                sb.AppendLine("─────────────────────────────")
                sb.AppendLine($"Booking Reference: {bookingRef}")
                sb.AppendLine($"Booking Status: Pending Admin Approval")
                sb.AppendLine($"Booking Date: {DateTime.Now:MMM dd, yyyy HH:mm}")
                sb.AppendLine()

                ' Customer Information
                sb.AppendLine("CUSTOMER INFORMATION")
                sb.AppendLine("────────────────────")
                sb.AppendLine($"Name: {CurrentUserName}")
                sb.AppendLine($"Email: {CurrentUserEmail}")
                sb.AppendLine($"Phone: {CurrentUserPhone}")
                sb.AppendLine()

                ' Trip Details
                sb.AppendLine("TRIP DETAILS")
                sb.AppendLine("────────────")
                sb.AppendLine($"Package Name: {selectedPackage.Title}")
                sb.AppendLine($"Destination: {selectedPackage.Location}")
                sb.AppendLine($"Package Type: {selectedPackage.PackageType}")
                sb.AppendLine($"Category: {selectedPackage.CategoryName}")
                sb.AppendLine($"Travel Start Date: {travelDate:MMM dd, yyyy}")
                sb.AppendLine($"Travel End Date: {endDate:MMM dd, yyyy}")
                sb.AppendLine($"Duration: {selectedPackage.Duration} days, {selectedPackage.DurationNights} nights")
                sb.AppendLine($"Number of Travelers: {numberOfPeople}")
                sb.AppendLine($"Available Slots: {selectedPackage.AvailableSlots}")
                sb.AppendLine()

                ' Selected Add-ons
                Dim hasAddOns As Boolean = False

                For Each chkBox As CheckBox In addOnCheckBoxes
                    If chkBox.Checked Then
                        If Not hasAddOns Then
                            sb.AppendLine("SELECTED ADD-ONS")
                            sb.AppendLine("────────────────")
                            hasAddOns = True
                        End If
                        Dim addOnIndex As Integer = CInt(chkBox.Tag)
                        Dim addOnCost = addOns(addOnIndex).Price * numberOfPeople
                        sb.AppendLine($"• {addOns(addOnIndex).AddOnName}")
                        sb.AppendLine($"  Price: ₱{addOns(addOnIndex).Price:N2} x {numberOfPeople} = ₱{addOnCost:N2}")
                        addOnTotal += addOnCost
                    End If
                Next

                If hasAddOns Then
                    sb.AppendLine($"{"",-30}Add-ons Subtotal: ₱{addOnTotal:N2}")
                    sb.AppendLine()
                End If

                ' Pricing Breakdown
                sb.AppendLine("PRICING BREAKDOWN")
                sb.AppendLine("─────────────────")
                sb.AppendLine($"Base Package Price: ₱{selectedPackage.Price:N2}")
                sb.AppendLine($"Number of People: {numberOfPeople}")
                sb.AppendLine($"Package Subtotal: ₱{selectedPackage.Price:N2} x {numberOfPeople} = ₱{baseAmount:N2}")

                If hasAddOns Then
                    sb.AppendLine($"Add-ons Total: ₱{addOnTotal:N2}")
                End If

                sb.AppendLine($"{"",-30}────────────────")
                sb.AppendLine($"Subtotal (Before Discount): ₱{subtotal:N2}")

                ' Discount information
                If discountApplied > 0 AndAlso appliedPromoDetails IsNot Nothing Then
                    sb.AppendLine()
                    sb.AppendLine("DISCOUNT APPLIED")
                    sb.AppendLine("────────────────")
                    sb.AppendLine($"Promo Type: {appliedPromoDetails.PromoTypeName}")
                    sb.AppendLine($"Discount Percentage: {appliedPromoDetails.DiscountPercentage}%")
                    sb.AppendLine($"Discount Amount: -₱{discountApplied:N2}")
                    sb.AppendLine($"You Saved: ₱{discountApplied:N2}")
                    sb.AppendLine()
                    sb.AppendLine($"{"",-30}════════════════")
                    sb.AppendLine($"FINAL TOTAL AMOUNT: ₱{finalAmount:N2}")
                    sb.AppendLine($"{"",-30}════════════════")
                Else
                    sb.AppendLine($"Discount Applied: None")
                    sb.AppendLine($"{"",-30}════════════════")
                    sb.AppendLine($"TOTAL AMOUNT: ₱{finalAmount:N2}")
                    sb.AppendLine($"{"",-30}════════════════")
                End If

                sb.AppendLine()

                ' Payment Information
                sb.AppendLine("PAYMENT INFORMATION")
                sb.AppendLine("───────────────────")
                sb.AppendLine($"Payment Method: {cmbPaymentMethod.SelectedItem}")
                sb.AppendLine($"Payment Status: Pending")
                sb.AppendLine($"Amount Due: ₱{finalAmount:N2}")
                sb.AppendLine()
            End If

            ' Important Notes
            sb.AppendLine("IMPORTANT NOTES")
            sb.AppendLine("───────────────")
            sb.AppendLine("• Your booking is currently PENDING and requires admin approval")
            sb.AppendLine("• You will receive a confirmation email once your booking is approved")
            sb.AppendLine("• Please keep your booking reference number for future inquiries")
            sb.AppendLine("• Contact our customer service for any questions or changes")
            sb.AppendLine("• Booking slots are reserved temporarily until admin approval")

            If discountAmount > 0 AndAlso appliedPromoDetails IsNot Nothing Then
                sb.AppendLine($"• Your {appliedPromoDetails.PromoTypeName} discount has been successfully applied")
            End If

            sb.AppendLine()
            sb.AppendLine("TERMS AND CONDITIONS")
            sb.AppendLine("───────────────────")
            sb.AppendLine("• Cancellation policy applies as per company terms")
            sb.AppendLine("• Changes to booking may incur additional charges")
            sb.AppendLine("• Travel dates are subject to availability")
            sb.AppendLine("• All prices are in Philippine Pesos (₱)")
            sb.AppendLine()

            ' Footer
            sb.AppendLine("================================================================")
            sb.AppendLine("           Thank you for choosing LakbayPH Travel and Tours!")
            sb.AppendLine("                Visit us at: www.lakbayphtravel.com")
            sb.AppendLine("              Email: info@lakbayphtravel.com")
            sb.AppendLine("                Phone: +63 XXX XXXX XXX")
            sb.AppendLine("================================================================")
            sb.AppendLine($"Receipt generated on: {DateTime.Now:MMMM dd, yyyy 'at' hh:mm:ss tt}")
            sb.AppendLine()

            Return sb.ToString()

        Catch ex As Exception
            MessageBox.Show("Error building receipt content: " & ex.Message, "Content Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return "Error generating receipt content."
        End Try
    End Function

    Private Function PackageExistsInDatabase(packageID As Integer) As Boolean
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                ' Check both PackageID and IsActive status
                Dim query As String = "SELECT COUNT(*) FROM TourPackages WHERE PackageID = @PackageID AND IsActive = 1 AND Status = 'active'"
                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@PackageID", packageID)
                    Dim exists As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                    Return exists > 0
                End Using
            End Using
        Catch ex As Exception
            ' Log error and assume package doesn't exist to be safe
            Console.WriteLine("Error checking package existence: " & ex.Message)
            Return False
        End Try
    End Function

    Private Sub ResetForm()
        Try
            LoadData()
            LoadTourCategories()
            cmbCategories.SelectedIndex = 0
            cmbPackages.SelectedIndex = 0
            nudNumberOfPeople.Value = 1
            cmbPaymentMethod.SelectedIndex = 0
            cmbPromoDiscount.SelectedIndex = 0
            dtpBookingDate.Value = DateTime.Today
            ClearAddOns()
            appliedPromoDetails = Nothing ' Clear applied promo details
            CalculateTotal()
        Catch ex As Exception
            MessageBox.Show("Error resetting form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        Dim result As DialogResult = MessageBox.Show("Are you sure you want to clear all form data?",
                                                "Clear Form", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If result = DialogResult.Yes Then
            ResetForm()
        End If
    End Sub

    Private Sub btnAddToList_Click(sender As Object, e As EventArgs) Handles btnAddToList.Click
        If ValidateFormForList() Then
            Dim selectedPackage = tourPackage(cmbPackages.SelectedIndex - 1)

            ' Build add-ons string and IDs
            Dim addOnsList As String = "None"
            Dim selectedAddOnIDs As New List(Of Integer)()
            If addOnCheckBoxes.Any(Function(chk) chk.Checked) Then
                Dim selectedAddOns As New List(Of String)()
                For Each chkBox As CheckBox In addOnCheckBoxes
                    If chkBox.Checked Then
                        Dim addOnIndex As Integer = CInt(chkBox.Tag)
                        selectedAddOns.Add(addOns(addOnIndex).AddOnName)
                        selectedAddOnIDs.Add(addOns(addOnIndex).AddOnID)
                    End If
                Next
                addOnsList = String.Join(", ", selectedAddOns)
            End If

            ' Create unique identifier
            Dim addOnIDsString = String.Join(",", selectedAddOnIDs.OrderBy(Function(x) x))
            Dim uniqueID = $"{CurrentUserID}_{selectedPackage.PackageID}_{nudNumberOfPeople.Value}_{dtpBookingDate.Value:yyyyMMdd}_{addOnIDsString}"

            ' Check if this combination already exists in the list
            Dim existingItem = BookingList.FirstOrDefault(Function(b) b.UniqueID = uniqueID)

            If existingItem IsNot Nothing Then
                MessageBox.Show("This package with the same options is already in your booking list.",
                          "Duplicate Item",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Information)
                Return
            End If

            ' Create booking item
            Dim bookingItem As New BookingItem() With {
            .PackageName = selectedPackage.Title,
            .PackageType = selectedPackage.PackageType,
            .NumberOfPeople = CInt(nudNumberOfPeople.Value),
            .AddOns = addOnsList,
            .TotalPrice = currentTotal,
            .BookingDate = dtpBookingDate.Value,
            .PackageID = selectedPackage.PackageID,
            .UserID = CurrentUserID,
            .UniqueID = uniqueID
        }

            ' Add to list
            BookingList.Add(bookingItem)

            ' Show success message with item count
            MessageBox.Show($"Booking added to list successfully!{vbCrLf}{vbCrLf}" &
                   $"Total items in list: {BookingList.Count}",
                   "Added to List",
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Information)

            ' Ask if user wants to view the list
            Dim viewResult As DialogResult = MessageBox.Show(
            "Would you like to view your booking list now?",
            "View Booking List",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question)

            If viewResult = DialogResult.Yes Then
                ShowBookingList()
            End If
        End If
    End Sub
    Private Sub ShowBookingList()
        Try
            ' Filter bookings for current user only
            Dim userBookings = BookingList.Where(Function(b) b.UserID = CurrentUserID).ToList()

            If userBookings.Count = 0 Then
                MessageBox.Show("Your booking list is empty.", "Empty List",
                          MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            ' Create and show booking list form
            Dim listForm As New BookingListForm()
            listForm.LoadBookings(userBookings)
            listForm.Show()

        Catch ex As Exception
            MessageBox.Show($"Error displaying booking list: {ex.Message}", "Error",
                       MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function ValidateFormForList() As Boolean
        If CurrentUserID <= 0 Then
            MessageBox.Show("No user is logged in. Please log in first.", "Authentication Required",
                       MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        If cmbCategories.SelectedIndex <= 0 Then
            MessageBox.Show("Please select a category.", "Validation Error",
                       MessageBoxButtons.OK, MessageBoxIcon.Warning)
            cmbCategories.Focus()
            Return False
        End If

        If cmbPackages.SelectedIndex <= 0 Or cmbPackages.SelectedIndex > tourPackage.Count Then
            MessageBox.Show("Please select a tour package.", "Validation Error",
                       MessageBoxButtons.OK, MessageBoxIcon.Warning)
            cmbPackages.Focus()
            Return False
        End If

        If nudNumberOfPeople.Value < 1 Then
            MessageBox.Show("Number of people must be at least 1.", "Validation Error",
                       MessageBoxButtons.OK, MessageBoxIcon.Warning)
            nudNumberOfPeople.Focus()
            Return False
        End If

        If dtpBookingDate.Value < DateTime.Today Then
            MessageBox.Show("Booking date cannot be in the past.", "Validation Error",
                       MessageBoxButtons.OK, MessageBoxIcon.Warning)
            dtpBookingDate.Focus()
            Return False
        End If

        ' Check if total has been calculated
        If currentTotal <= 0 Then
            MessageBox.Show("Please calculate the total first.", "Validation Error",
                       MessageBoxButtons.OK, MessageBoxIcon.Warning)
            btnCalculateTotal.Focus()
            Return False
        End If

        Return True
    End Function

    Public Shared Sub ClearBookingList(Optional userID As Integer = 0)
        If userID > 0 Then
            ' Remove only bookings for specific user
            BookingList = BookingList.Where(Function(b) b.UserID <> userID).ToList()
        Else
            ' Clear entire list
            BookingList.Clear()
        End If
    End Sub

    Public Function GetUserBookingCount() As Integer
        Return BookingList.Where(Function(b) b.UserID = CurrentUserID).Count()
    End Function

    Private Function GetNextBookingId(conn As MySqlConnection, transaction As MySqlTransaction) As Long
        Try
            ' Get the next available booking number by checking existing BookingReference values
            Dim currentYear As String = DateTime.Now.Year.ToString()

            ' Check if BookingReference column exists first
            Dim checkColumnQuery As String = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Bookings' AND COLUMN_NAME = 'BookingReference'"
            Using checkCmd As New MySqlCommand(checkColumnQuery, conn, transaction)
                Dim columnExists As Long = CLng(checkCmd.ExecuteScalar())

                If columnExists > 0 Then
                    ' Column exists, get max from BookingReference
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
                    ' Column doesn't exist, use BookingID based approach
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
            ' Fallback: use timestamp-based ID to ensure uniqueness
            Return (DateTime.Now.Ticks Mod 1000000) + 1
        End Try
    End Function

    Private Function InsertMainBookingWithPromo(conn As MySqlConnection, transaction As MySqlTransaction, selectedPackage As TourPackages, travelDate As DateTime, endDate As DateTime, bookingRef As String) As Long
        Try
            Dim bookingId As Long = 0

            Dim insertQuery As String = "INSERT INTO Bookings (UserID, PackageID, NumberOfPeople, PromoID, TravelDate, EndDate, BookingDate, Status, BookingReference) " &
                                        "VALUES (@UserID, @PackageID, @NumberOfPeople, @PromoID, @TravelDate, @EndDate, @BookingDate, @Status, @BookingReference)"

            Using cmd As New MySqlCommand(insertQuery, conn, transaction)
                cmd.Parameters.Add("@UserID", MySqlDbType.Int32).Value = CurrentUserID
                cmd.Parameters.Add("@PackageID", MySqlDbType.Int64).Value = selectedPackage.PackageID
                cmd.Parameters.Add("@NumberOfPeople", MySqlDbType.Int32).Value = CInt(nudNumberOfPeople.Value)
                cmd.Parameters.Add("@PromoID", MySqlDbType.Int32).Value = If(selectedPromoID > 0, selectedPromoID, DBNull.Value)
                cmd.Parameters.Add("@TravelDate", MySqlDbType.Date).Value = travelDate
                cmd.Parameters.Add("@EndDate", MySqlDbType.Date).Value = endDate
                cmd.Parameters.Add("@BookingDate", MySqlDbType.DateTime).Value = DateTime.Now
                cmd.Parameters.Add("@Status", MySqlDbType.VarChar, 20).Value = "Pending"

                cmd.Parameters.Add("@BookingReference", MySqlDbType.VarChar, 20).Value = bookingRef

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

    Private Sub InsertBookingPayments(conn As MySqlConnection, transaction As MySqlTransaction, bookingID As Long)
        Try
            ' Check if BookingPayments table exists
            Dim tableExistsQuery As String = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'BookingPayments'"
            Using checkCmd As New MySqlCommand(tableExistsQuery, conn, transaction)
                Dim tableExists As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())

                If tableExists > 0 Then
                    Dim paymentQuery As String = "INSERT INTO BookingPayments (BookingID, PaymentMethod, PaymentStatus, PaymentAmount) " &
                                           "VALUES (@BookingID, @PaymentMethod, 'Pending', @Amount)"

                    Using cmd As New MySqlCommand(paymentQuery, conn, transaction)
                        cmd.Parameters.AddWithValue("@BookingID", bookingID)
                        cmd.Parameters.AddWithValue("@PaymentMethod", cmbPaymentMethod.SelectedItem.ToString())
                        cmd.Parameters.AddWithValue("@Amount", currentTotal)
                        cmd.ExecuteNonQuery()
                    End Using
                End If
            End Using
        Catch ex As Exception
            ' Log the error but don't stop the booking process
            Console.WriteLine("Warning: Could not insert payment record: " & ex.Message)
        End Try
    End Sub

    Private Function CheckIfPromoColumnsExist(conn As MySqlConnection, transaction As MySqlTransaction) As Boolean
        Try
            Dim query As String = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Bookings' AND COLUMN_NAME IN ('PromoID', 'OriginalAmount', 'DiscountAmount', 'FinalAmount')"
            Using cmd As New MySqlCommand(query, conn, transaction)
                Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                Return count = 4 ' All 4 promo columns exist
            End Using
        Catch
            Return False
        End Try
    End Function

    Private Function CheckIfBookingReferenceExists(conn As MySqlConnection, transaction As MySqlTransaction) As Boolean
        Try
            Dim query As String = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Bookings' AND COLUMN_NAME = 'BookingReference'"
            Using cmd As New MySqlCommand(query, conn, transaction)
                Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                Return count > 0
            End Using
        Catch
            Return False
        End Try
    End Function

    Private Sub InsertBookingAddOns(conn As MySqlConnection, transaction As MySqlTransaction, bookingID As Long)
        Try
            ' Check if BookingAddOns table exists
            Dim tableExistsQuery As String = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'BookingAddOns'"
            Using checkCmd As New MySqlCommand(tableExistsQuery, conn, transaction)
                Dim tableExists As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())

                If tableExists > 0 Then
                    For Each chkBox As CheckBox In addOnCheckBoxes
                        If chkBox.Checked Then
                            Dim addOnIndex As Integer = CInt(chkBox.Tag)
                            Dim addOn = addOns(addOnIndex)

                            Dim addOnQuery As String = "INSERT INTO BookingAddOns (BookingID, AddOnID, Quantity, UnitPrice, TotalPrice) " &
                                                  "VALUES (@BookingID, @AddOnID, @Quantity, @UnitPrice, @TotalPrice)"

                            Using cmd As New MySqlCommand(addOnQuery, conn, transaction)
                                cmd.Parameters.AddWithValue("@BookingID", bookingID)
                                cmd.Parameters.AddWithValue("@AddOnID", addOn.AddOnID)
                                cmd.Parameters.AddWithValue("@Quantity", nudNumberOfPeople.Value)
                                cmd.Parameters.AddWithValue("@UnitPrice", addOn.Price)
                                cmd.Parameters.AddWithValue("@TotalPrice", addOn.Price * nudNumberOfPeople.Value)
                                cmd.ExecuteNonQuery()
                            End Using
                        End If
                    Next
                End If
            End Using
        Catch ex As Exception
            ' Log the error but don't stop the booking process
            Console.WriteLine("Warning: Could not insert add-on records: " & ex.Message)
        End Try
    End Sub
    Private Sub InsertBookingHistory(conn As MySqlConnection, transaction As MySqlTransaction, bookingID As Long, bookingReference As String)
        Try
            ' Check if BookingHistory table exists
            Dim tableExistsQuery As String = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'BookingHistory'"
            Using checkCmd As New MySqlCommand(tableExistsQuery, conn, transaction)
                Dim tableExists As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())

                If tableExists > 0 Then
                    Dim historyQuery As String = "INSERT INTO BookingHistory (BookingID, UserID, ActionType, ActionDescription, PreviousStatus, NewStatus, ModifiedBy) " &
                                           "VALUES (@BookingID, @UserID, 'Created', 'Booking created and pending admin approval', NULL, 'Pending', @UserID)"

                    Using cmd As New MySqlCommand(historyQuery, conn, transaction)
                        cmd.Parameters.AddWithValue("@BookingID", bookingID)
                        cmd.Parameters.AddWithValue("@UserID", CurrentUserID)
                        cmd.ExecuteNonQuery()
                    End Using
                End If
            End Using
        Catch ex As Exception
            ' Log the error but don't stop the booking process
            Console.WriteLine("Warning: Could not insert history record: " & ex.Message)
        End Try
    End Sub

    Private Sub UpdatePackageSlots(conn As MySqlConnection, transaction As MySqlTransaction, packageID As Integer)
        Try
            ' Optional: Add validation to ensure we don't exceed MaxSlots
            Dim validationQuery As String = "SELECT " &
                                      "p.MaxSlots, COALESCE(SUM(b.NumberOfPeople), 0) as CurrentBookings " &
                                      "FROM TourPackages p " &
                                      "LEFT JOIN Bookings b ON p.PackageID = b.PackageID " &
                                      "AND b.Status IN ('Pending', 'Confirmed') " &
                                      "WHERE p.PackageID = @PackageID " &
                                      "GROUP BY p.PackageID, p.MaxSlots"

            Using cmd As New MySqlCommand(validationQuery, conn, transaction)
                cmd.Parameters.AddWithValue("@PackageID", packageID)
                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        Dim maxSlots As Integer = Convert.ToInt32(reader("MaxSlots"))
                        Dim currentBookings As Integer = Convert.ToInt32(reader("CurrentBookings"))
                        Dim newBookings As Integer = CInt(nudNumberOfPeople.Value)

                        If (currentBookings + newBookings) > maxSlots Then
                            Throw New Exception($"Booking would exceed maximum capacity. Available: {maxSlots - currentBookings}, Requested: {newBookings}")
                        End If
                    End If
                End Using
            End Using
        Catch ex As Exception
            Throw New Exception("Slot validation failed: " & ex.Message)
        End Try
    End Sub

    Private Function GetBookingAmounts(bookingID As Long) As Dictionary(Of String, Decimal)
        Dim amounts As New Dictionary(Of String, Decimal)

        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()

                ' Use the BookingAmounts view from your database
                Dim query As String = "SELECT BaseAmount, AddOnTotal, SubTotal, DiscountAmount, FinalAmount " &
                                "FROM BookingAmounts WHERE BookingID = @BookingID"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@BookingID", bookingID)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            amounts("BaseAmount") = Convert.ToDecimal(reader("BaseAmount"))
                            amounts("AddOnTotal") = Convert.ToDecimal(reader("AddOnTotal"))
                            amounts("SubTotal") = Convert.ToDecimal(reader("SubTotal"))
                            amounts("DiscountAmount") = Convert.ToDecimal(reader("DiscountAmount"))
                            amounts("FinalAmount") = Convert.ToDecimal(reader("FinalAmount"))
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            ' Return zero amounts if calculation fails
            amounts("BaseAmount") = 0
            amounts("AddOnTotal") = 0
            amounts("SubTotal") = 0
            amounts("DiscountAmount") = 0
            amounts("FinalAmount") = 0
        End Try

        Return amounts
    End Function

    Private Sub UpdateBookingStatusToConfirmed(bookingID As Long)
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Using transaction As MySqlTransaction = conn.BeginTransaction()
                    Try
                        ' Update booking status to Confirmed
                        Dim updateStatusQuery As String = "UPDATE Bookings SET Status = 'Pending', UpdatedAt = NOW() WHERE BookingID = @BookingID"
                        Using cmd As New MySqlCommand(updateStatusQuery, conn, transaction)
                            cmd.Parameters.AddWithValue("@BookingID", bookingID)
                            cmd.ExecuteNonQuery()
                        End Using

                        ' Add history entry for status update
                        Dim historyQuery As String = "INSERT INTO BookingHistory (BookingID, UserID, ActionType, ActionDescription, PreviousStatus, NewStatus, ModifiedBy) " &
                                           "VALUES (@BookingID, @UserID, 'Modified', 'Status updated to Confirmed after booking confirmation', 'Pending', 'Confirmed', @UserID)"

                        Using historyCmd As New MySqlCommand(historyQuery, conn, transaction)
                            historyCmd.Parameters.AddWithValue("@BookingID", bookingID)
                            historyCmd.Parameters.AddWithValue("@UserID", CurrentUserID)
                            historyCmd.ExecuteNonQuery()
                        End Using

                        transaction.Commit()

                    Catch ex As Exception
                        transaction.Rollback()
                        Throw ex
                    End Try
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error updating booking status: " & ex.Message, "Status Update Error",
                       MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ShowBookingConfirmation(bookingRef As String, package As Object, travelDate As Date,
                              endDate As Date, numberOfPeople As Integer, totalAmount As Decimal,
                              paymentMethod As String)

        ' Create confirmation form
        Dim confirmForm As New Form()
        confirmForm.Text = "Booking Confirmation"
        confirmForm.Size = New Size(650, 900)
        confirmForm.StartPosition = FormStartPosition.CenterParent
        confirmForm.FormBorderStyle = FormBorderStyle.FixedDialog
        confirmForm.MaximizeBox = False
        confirmForm.MinimizeBox = False
        confirmForm.BackColor = Color.White
        confirmForm.Font = New Font("Segoe UI", 9)

        ' Success Header Panel
        Dim headerPanel As New Panel()
        headerPanel.Location = New Point(0, 0)
        headerPanel.Size = New Size(650, 100)
        headerPanel.BackColor = Color.FromArgb(34, 139, 34) ' Forest Green
        confirmForm.Controls.Add(headerPanel)

        ' Success Icon (using text symbol)
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
        successTitle.Text = "Booking Submitted!"
        successTitle.Location = New Point(130, 25)
        successTitle.Size = New Size(400, 35)
        successTitle.Font = New Font("Segoe UI", 20, FontStyle.Bold)
        successTitle.ForeColor = Color.White
        headerPanel.Controls.Add(successTitle)

        ' Success Subtitle
        Dim successSubtitle As New Label()
        successSubtitle.Text = "Your booking is pending admin approval"
        successSubtitle.Location = New Point(130, 60)
        successSubtitle.Size = New Size(400, 20)
        successSubtitle.Font = New Font("Segoe UI", 11)
        successSubtitle.ForeColor = Color.White
        headerPanel.Controls.Add(successSubtitle)

        ' Main Content Panel
        Dim contentPanel As New Panel()
        contentPanel.Location = New Point(30, 120)
        contentPanel.Size = New Size(590, 620)
        contentPanel.BackColor = Color.FromArgb(248, 249, 250)
        contentPanel.BorderStyle = BorderStyle.FixedSingle
        contentPanel.AutoScroll = True
        confirmForm.Controls.Add(contentPanel)

        Dim yPos As Integer = 20

        ' Booking Reference (Highlighted)
        Dim refPanel As New Panel()
        refPanel.Location = New Point(20, yPos)
        refPanel.Size = New Size(550, 50)
        refPanel.BackColor = Color.FromArgb(230, 247, 255)
        refPanel.BorderStyle = BorderStyle.FixedSingle
        contentPanel.Controls.Add(refPanel)

        Dim refLabel As New Label()
        refLabel.Text = "Booking Reference Number"
        refLabel.Location = New Point(15, 8)
        refLabel.Size = New Size(200, 15)
        refLabel.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        refLabel.ForeColor = Color.FromArgb(70, 130, 180)
        refPanel.Controls.Add(refLabel)

        Dim refValue As New Label()
        refValue.Text = bookingRef
        refValue.Location = New Point(15, 25)
        refValue.Size = New Size(300, 20)
        refValue.Font = New Font("Segoe UI", 14, FontStyle.Bold)
        refValue.ForeColor = Color.FromArgb(70, 130, 180)
        refPanel.Controls.Add(refValue)

        yPos += 70

        ' Trip Details Section
        CreateConfirmationSection(contentPanel, "TRIP DETAILS", yPos)
        yPos += 40
        CreateConfirmationRow(contentPanel, "Package:", package.Title, yPos)
        yPos += 25

        ' Get location from PackageLocations table or use default
        Dim packageLocation As String = GetPackageLocation(package.PackageID)
        CreateConfirmationRow(contentPanel, "Destination:", packageLocation, yPos)
        yPos += 25
        CreateConfirmationRow(contentPanel, "Travel Dates:", $"{travelDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}", yPos)
        yPos += 25
        CreateConfirmationRow(contentPanel, "Duration:", $"{(endDate - travelDate).Days + 1} days", yPos)
        yPos += 25
        CreateConfirmationRow(contentPanel, "Number of People:", numberOfPeople.ToString(), yPos)
        yPos += 35

        ' Booking Details Section
        CreateConfirmationSection(contentPanel, "BOOKING DETAILS", yPos)
        yPos += 40
        CreateConfirmationRow(contentPanel, "Booking Date:", DateTime.Now.ToString("MMM dd, yyyy HH:mm"), yPos)
        yPos += 25
        CreateConfirmationRow(contentPanel, "Payment Method:", paymentMethod, yPos)
        yPos += 35

        ' Add-ons section (if any selected)
        Dim selectedAddOns As New List(Of String)()
        Dim addOnTotal As Decimal = 0

        For Each chkBox As CheckBox In addOnCheckBoxes
            If chkBox.Checked Then
                Dim addOnIndex As Integer = CInt(chkBox.Tag)
                Dim addOnCost = addOns(addOnIndex).Price * numberOfPeople
                selectedAddOns.Add($"{addOns(addOnIndex).AddOnName} - ₱{addOns(addOnIndex).Price:N2} x {numberOfPeople} = ₱{addOnCost:N2}")
                addOnTotal += addOnCost
            End If
        Next

        If selectedAddOns.Count > 0 Then
            CreateConfirmationSection(contentPanel, "ADD-ONS SELECTED", yPos)
            yPos += 35
            For Each addOnText In selectedAddOns
                CreateConfirmationRow(contentPanel, "•", addOnText, yPos)
                yPos += 22
            Next
            CreateConfirmationRow(contentPanel, "Add-ons Subtotal:", $"₱{addOnTotal:N2}", yPos, True)
            yPos += 35
        End If

        ' Enhanced Pricing Breakdown Section
        CreateConfirmationSection(contentPanel, "PRICING BREAKDOWN", yPos)
        yPos += 40

        ' Package cost breakdown
        Dim packageCost = package.Price * numberOfPeople
        CreateConfirmationRow(contentPanel, "Package Cost:", $"₱{package.Price:N2} x {numberOfPeople} = ₱{packageCost:N2}", yPos)
        yPos += 25

        If addOnTotal > 0 Then
            CreateConfirmationRow(contentPanel, "Add-ons Total:", $"₱{addOnTotal:N2}", yPos)
            yPos += 25
        End If

        ' Subtotal before discount
        CreateConfirmationRow(contentPanel, "Subtotal:", $"₱{originalAmount:N2}", yPos, True)
        yPos += 30

        ' Discount section using appliedPromoDetails
        If discountAmount > 0 AndAlso appliedPromoDetails IsNot Nothing Then
            ' Create a highlighted discount panel
            Dim discountPanel As New Panel()
            discountPanel.Location = New Point(20, yPos)
            discountPanel.Size = New Size(550, 60)
            discountPanel.BackColor = Color.FromArgb(255, 245, 235) ' Light orange background
            discountPanel.BorderStyle = BorderStyle.FixedSingle
            contentPanel.Controls.Add(discountPanel)

            ' Discount header
            Dim discountHeader As New Label()
            discountHeader.Text = $"🎉 {appliedPromoDetails.PromoTypeName} DISCOUNT APPLIED"
            discountHeader.Location = New Point(15, 8)
            discountHeader.Size = New Size(400, 20)
            discountHeader.Font = New Font("Segoe UI", 10, FontStyle.Bold)
            discountHeader.ForeColor = Color.FromArgb(255, 87, 34)
            discountPanel.Controls.Add(discountHeader)

            ' Discount details
            Dim discountDetails As New Label()
            discountDetails.Text = $"{appliedPromoDetails.DiscountPercentage}% off"
            discountDetails.Location = New Point(15, 30)
            discountDetails.Size = New Size(200, 20)
            discountDetails.Font = New Font("Segoe UI", 9)
            discountDetails.ForeColor = Color.FromArgb(100, 100, 100)
            discountPanel.Controls.Add(discountDetails)

            ' Discount amount (right aligned)
            Dim discountAmountLabel As New Label()
            discountAmountLabel.Text = $"-₱{discountAmount:N2}"
            discountAmountLabel.Location = New Point(350, 20)
            discountAmountLabel.Size = New Size(180, 25)
            discountAmountLabel.Font = New Font("Segoe UI", 14, FontStyle.Bold)
            discountAmountLabel.ForeColor = Color.FromArgb(255, 87, 34)
            discountAmountLabel.TextAlign = ContentAlignment.MiddleRight
            discountPanel.Controls.Add(discountAmountLabel)

            yPos += 80

            ' Final amount with savings highlight
            Dim finalPanel As New Panel()
            finalPanel.Location = New Point(20, yPos)
            finalPanel.Size = New Size(550, 45)
            finalPanel.BackColor = Color.FromArgb(232, 245, 233) ' Light green background
            finalPanel.BorderStyle = BorderStyle.FixedSingle
            contentPanel.Controls.Add(finalPanel)

            Dim finalLabel As New Label()
            finalLabel.Text = "TOTAL AMOUNT (After Discount):"
            finalLabel.Location = New Point(15, 12)
            finalLabel.Size = New Size(300, 20)
            finalLabel.Font = New Font("Segoe UI", 12, FontStyle.Bold)
            finalLabel.ForeColor = Color.FromArgb(27, 94, 32)
            finalPanel.Controls.Add(finalLabel)

            Dim finalValue As New Label()
            finalValue.Text = $"₱{totalAmount:N2}"
            finalValue.Location = New Point(350, 10)
            finalValue.Size = New Size(180, 25)
            finalValue.Font = New Font("Segoe UI", 16, FontStyle.Bold)
            finalValue.ForeColor = Color.FromArgb(27, 94, 32)
            finalValue.TextAlign = ContentAlignment.MiddleRight
            finalPanel.Controls.Add(finalValue)

            ' Savings indicator
            Dim savingsLabel As New Label()
            savingsLabel.Text = $"You saved ₱{discountAmount:N2}!"
            savingsLabel.Location = New Point(350, 35)
            savingsLabel.Size = New Size(180, 15)
            savingsLabel.Font = New Font("Segoe UI", 8, FontStyle.Italic)
            savingsLabel.ForeColor = Color.FromArgb(76, 175, 80)
            savingsLabel.TextAlign = ContentAlignment.MiddleRight
            finalPanel.Controls.Add(savingsLabel)

            yPos += 60
        Else
            ' No discount applied
            CreateConfirmationRow(contentPanel, "Discount Applied:", "None", yPos)
            yPos += 25

            ' Final amount without discount
            Dim finalPanel As New Panel()
            finalPanel.Location = New Point(20, yPos)
            finalPanel.Size = New Size(550, 35)
            finalPanel.BackColor = Color.FromArgb(227, 242, 253)
            finalPanel.BorderStyle = BorderStyle.FixedSingle
            contentPanel.Controls.Add(finalPanel)

            Dim finalLabel As New Label()
            finalLabel.Text = "TOTAL AMOUNT:"
            finalLabel.Location = New Point(15, 8)
            finalLabel.Size = New Size(200, 20)
            finalLabel.Font = New Font("Segoe UI", 12, FontStyle.Bold)
            finalLabel.ForeColor = Color.FromArgb(25, 118, 210)
            finalPanel.Controls.Add(finalLabel)

            Dim finalValue As New Label()
            finalValue.Text = $"₱{totalAmount:N2}"
            finalValue.Location = New Point(350, 6)
            finalValue.Size = New Size(180, 22)
            finalValue.Font = New Font("Segoe UI", 14, FontStyle.Bold)
            finalValue.ForeColor = Color.FromArgb(25, 118, 210)
            finalValue.TextAlign = ContentAlignment.MiddleRight
            finalPanel.Controls.Add(finalValue)

            yPos += 50
        End If

        yPos += 20

        ' Important Notes Section
        CreateConfirmationSection(contentPanel, "IMPORTANT NOTES", yPos)
        yPos += 35

        Dim notesText As String = "• Your booking is currently PENDING and requires admin approval" & vbCrLf &
                         "• You will receive a confirmation email once approved" & vbCrLf &
                         "• Please keep your booking reference number for future inquiries" & vbCrLf &
                         "• Contact customer service for any questions or changes"

        If discountAmount > 0 AndAlso appliedPromoDetails IsNot Nothing Then
            notesText &= vbCrLf & $"• Your {appliedPromoDetails.PromoTypeName} discount has been successfully applied to this booking"
        End If

        Dim notesLabel As New Label()
        notesLabel.Text = notesText
        notesLabel.Location = New Point(30, yPos)
        notesLabel.Size = New Size(530, 100)
        notesLabel.Font = New Font("Segoe UI", 9)
        notesLabel.ForeColor = Color.FromArgb(100, 100, 100)
        contentPanel.Controls.Add(notesLabel)

        ' Buttons Panel
        Dim buttonPanel As New Panel()
        buttonPanel.Location = New Point(0, 760)
        buttonPanel.Size = New Size(650, 80)
        buttonPanel.BackColor = Color.FromArgb(248, 249, 250)
        confirmForm.Controls.Add(buttonPanel)

        ' Print Button
        Dim btnPrint As New Button()
        btnPrint.Text = "Print Receipt"
        btnPrint.Location = New Point(200, 20)
        btnPrint.Size = New Size(120, 35)
        btnPrint.BackColor = Color.FromArgb(70, 130, 180)
        btnPrint.ForeColor = Color.White
        btnPrint.FlatStyle = FlatStyle.Flat
        btnPrint.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        AddHandler btnPrint.Click, Sub() PrintBookingConfirmation(confirmForm)
        buttonPanel.Controls.Add(btnPrint)

        ' OK Button - FIXED: Properly close the form
        Dim btnOK As New Button()
        btnOK.Text = "Close"
        btnOK.Location = New Point(330, 20)
        btnOK.Size = New Size(120, 35)
        btnOK.BackColor = Color.FromArgb(34, 139, 34)
        btnOK.ForeColor = Color.White
        btnOK.FlatStyle = FlatStyle.Flat
        btnOK.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        AddHandler btnOK.Click, Sub()
                                    confirmForm.DialogResult = DialogResult.OK
                                    confirmForm.Close()
                                    Me.Close()
                                End Sub
        buttonPanel.Controls.Add(btnOK)

        ' Show the confirmation dialog modally
        confirmForm.ShowDialog()
        confirmForm.Dispose() ' Ensure proper cleanup
    End Sub

    ' Helper method to create section headers
    Private Sub CreateConfirmationSection(parent As Panel, title As String, yPos As Integer)
        Dim sectionLabel As New Label()
        sectionLabel.Text = title
        sectionLabel.Location = New Point(20, yPos)
        sectionLabel.Size = New Size(550, 25)
        sectionLabel.Font = New Font("Segoe UI", 11, FontStyle.Bold)
        sectionLabel.ForeColor = Color.FromArgb(70, 130, 180)
        sectionLabel.BackColor = Color.FromArgb(240, 248, 255)
        sectionLabel.BorderStyle = BorderStyle.FixedSingle
        sectionLabel.TextAlign = ContentAlignment.MiddleLeft
        sectionLabel.Padding = New Padding(10, 0, 0, 0)
        parent.Controls.Add(sectionLabel)
    End Sub

    ' Helper method to create confirmation rows
    Private Sub CreateConfirmationRow(parent As Panel, label As String, value As String, yPos As Integer, Optional isBold As Boolean = False)
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
        lblValue.Size = New Size(320, 20)
        lblValue.Font = If(isBold, New Font("Segoe UI", 10, FontStyle.Bold), New Font("Segoe UI", 9))
        lblValue.ForeColor = If(isBold, Color.FromArgb(34, 139, 34), Color.Black)
        lblValue.TextAlign = ContentAlignment.MiddleRight
        parent.Controls.Add(lblValue)
    End Sub

    ' Helper method to get package location
    Private Function GetPackageLocation(packageID As Integer) As String
        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim query As String = "SELECT Location FROM PackageLocations WHERE PackageID = @PackageID LIMIT 1"
                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@PackageID", packageID)
                    Dim result = cmd.ExecuteScalar()
                    If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                        Return result.ToString()
                    Else
                        ' Fallback: get from TourPackages table
                        Dim fallbackQuery As String = "SELECT COALESCE(Location, 'Location TBD') as Location FROM TourPackages WHERE PackageID = @PackageID"
                        Using fallbackCmd As New MySqlCommand(fallbackQuery, conn)
                            fallbackCmd.Parameters.AddWithValue("@PackageID", packageID)
                            Dim fallbackResult = fallbackCmd.ExecuteScalar()
                            Return If(fallbackResult?.ToString(), "Location TBD")
                        End Using
                    End If
                End Using
            End Using
        Catch ex As Exception
            Return "Location TBD"
        End Try
    End Function

    ' Helper method to print booking confirmation
    Private Sub PrintBookingConfirmation(confirmForm As Form)
        Try
            ' Get the receipt content
            Dim receiptContent As String = BuildReceiptContent()

            ' Create a SaveFileDialog
            Dim saveDialog As New SaveFileDialog()
            saveDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            saveDialog.DefaultExt = "txt"
            saveDialog.FileName = $"BookingReceipt_{CurrentUserName.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.txt"

            ' Show the save dialog
            If saveDialog.ShowDialog() = DialogResult.OK Then
                ' Write the content to the file
                File.WriteAllText(saveDialog.FileName, receiptContent)

                ' Show success message
                MessageBox.Show($"Receipt exported successfully to:{vbCrLf}{saveDialog.FileName}",
                          "Export Complete",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Information)

                ' Optionally open the file in Notepad
                Dim result As DialogResult = MessageBox.Show("Would you like to open the receipt now?",
                                                     "Open Receipt",
                                                     MessageBoxButtons.YesNo,
                                                     MessageBoxIcon.Question)
                If result = DialogResult.Yes Then
                    Process.Start("notepad.exe", saveDialog.FileName)
                End If
            End If

        Catch ex As Exception
            MessageBox.Show($"Error exporting receipt: {ex.Message}", "Export Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class