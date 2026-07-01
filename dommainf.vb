Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

Public Class DomesticTravelForms
    Inherits Form
    Dim connectionString As String = "Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;"

    Private packages As List(Of TravelPackage)
    Private selectedPackage As TravelPackage = Nothing
    Private mainPanel As Panel
    Private packagesFlowPanel As Panel
    Private servicesPanel As Panel
    Private packageToShow As TravelPackage = Nothing
    Private filterPanel As Panel
    Public Property Rating As Double = 0.0
    Public Property ReviewCount As Integer = 0
    Public Property ImageFileName As String = ""

    Private originalPackages As List(Of TravelPackage)
    Private searchTextBox As TextBox

    Public Sub New(Optional package As TravelPackage = Nothing)
        packageToShow = package
        InitializePackages()
        InitializeComponents()

        Me.Controls.Add(mainPanel)
        SetupServicesSection()

        ' Force show top nav after everything is loaded
        AddHandler Me.Shown, Sub()
                                 Dim pnlTopNav = New Panel()
                                 pnlTopNav.Size = New Size(Me.Width, 90)
                                 pnlTopNav.Location = New Point(0, 0)
                                 pnlTopNav.BackColor = Color.White

                                 ' Logo
                                 Dim logo = New PictureBox()
                                 logo.Size = New Size(65, 65)
                                 logo.Location = New Point(60, 15)
                                 Try
                                     ' Try output directory first, then source directory
                                     Dim logoPath As String = Path.Combine(Application.StartupPath, "logo.png")
                                     If Not File.Exists(logoPath) Then
                                         logoPath = Path.Combine(Application.StartupPath, "..\..\logo.png")
                                     End If
                                     If File.Exists(logoPath) Then
                                         logo.Image = Image.FromFile(logoPath)
                                         logo.SizeMode = PictureBoxSizeMode.StretchImage
                                     Else
                                         logo.BackColor = Color.FromArgb(6, 41, 55)
                                         logo.BorderStyle = BorderStyle.None
                                     End If
                                 Catch
                                     logo.BackColor = Color.FromArgb(6, 41, 55)
                                 End Try
                                 pnlTopNav.Controls.Add(logo)

                                 ' Logo text
                                 Dim lblLogo = New Label()
                                 lblLogo.Text = "LakbayPH Travel + Tours"
                                 lblLogo.Font = New Font("Arial", 25, FontStyle.Bold)
                                 lblLogo.ForeColor = Color.FromArgb(6, 41, 55)
                                 lblLogo.Location = New Point(130, 15)
                                 lblLogo.Size = New Size(500, 40)
                                 lblLogo.TextAlign = ContentAlignment.MiddleLeft
                                 lblLogo.BackColor = Color.Transparent
                                 pnlTopNav.Controls.Add(lblLogo)

                                 ' Sub text
                                 Dim lblSubLogo = New Label()
                                 lblSubLogo.Text = "Discover the Philippines - Premium Domestic Travel Packages ⭐⭐⭐"
                                 lblSubLogo.Font = New Font("Arial", 12)
                                 lblSubLogo.ForeColor = Color.FromArgb(6, 41, 55)
                                 lblSubLogo.Location = New Point(130, 57)
                                 lblSubLogo.Size = New Size(500, 20)
                                 lblSubLogo.TextAlign = ContentAlignment.MiddleLeft
                                 lblSubLogo.BackColor = Color.Transparent
                                 pnlTopNav.Controls.Add(lblSubLogo)

                                 Dim backButton As New Button With {
                                     .Text = "← Back",
                                     .Font = New Font("Segoe UI", 12, FontStyle.Bold),
                                     .ForeColor = Color.White,
                                     .BackColor = Color.FromArgb(6, 41, 55),
                                     .Size = New Size(100, 40),
                                     .Location = New Point(pnlTopNav.Width - 200, 25),
                                     .FlatStyle = FlatStyle.Flat,
                                     .Cursor = Cursors.Hand
                                 }

                                 backButton.FlatAppearance.BorderSize = 0
                                 AddHandler backButton.Click, AddressOf BackButton_Click
                                 pnlTopNav.Controls.Add(backButton)

                                 Dim listButton As New Button With {
                                    .Text = "📋",
                                    .Size = New Size(40, 40),
                                    .Location = New Point(pnlTopNav.Width - 250, 25),
                                    .BackColor = Color.FromArgb(6, 41, 55),
                                    .ForeColor = Color.White,
                                    .FlatStyle = FlatStyle.Flat,
                                    .Font = New Font("Segoe UI", 12, FontStyle.Bold),
                                    .Cursor = Cursors.Hand,
                                    .Anchor = AnchorStyles.Top Or AnchorStyles.Right
                                }
                                 listButton.FlatAppearance.BorderSize = 0
                                 AddHandler listButton.Click, AddressOf ListButton_Click
                                 pnlTopNav.Controls.Add(listButton)

                                 Me.Controls.Add(pnlTopNav)
                                 pnlTopNav.BringToFront()

                                 ' Create filter panel
                                 filterPanel = New Panel With {
                                    .Size = New Size(Me.Width, 50),
                                    .Location = New Point(0, 90),
                                    .BackColor = Color.FromArgb(6, 41, 55),
                                    .Padding = New Padding(20, 10, 20, 10)
                                }

                                 Dim filterLabel As New Label With {
                                        .Text = "Sort By:",
                                        .Font = New Font("Segoe UI", 11, FontStyle.Bold),
                                        .ForeColor = Color.White,
                                        .Location = New Point(80, 15),
                                        .AutoSize = True
                                    }

                                 Dim priceLabel As New Label With {
                                    .Text = "Price:",
                                    .Font = New Font("Segoe UI", 10, FontStyle.Regular),
                                    .ForeColor = Color.White,
                                    .Location = New Point(160, 15),
                                    .AutoSize = True
                                }

                                 Dim priceDropdown As New ComboBox With {
                                    .Name = "priceDropdown",
                                    .Location = New Point(210, 12),
                                    .Size = New Size(80, 25),
                                    .DropDownStyle = ComboBoxStyle.DropDownList,
                                    .BackColor = Color.White,
                                    .Font = New Font("Segoe UI", 9)
                                }
                                 priceDropdown.Items.AddRange({"None", "🔼 Lowest", "🔽 Highest"})
                                 priceDropdown.SelectedIndex = 0  ' Set "None" as default

                                 Dim ratingLabel As New Label With {
                                    .Text = "Rating:",
                                    .Font = New Font("Segoe UI", 10, FontStyle.Regular),
                                    .ForeColor = Color.White,
                                    .Location = New Point(310, 15),
                                    .AutoSize = True
                                }

                                 Dim ratingDropdown As New ComboBox With {
                                    .Name = "ratingDropdown",
                                    .Location = New Point(370, 12),
                                    .Size = New Size(80, 25),
                                    .DropDownStyle = ComboBoxStyle.DropDownList,
                                    .BackColor = Color.White,
                                    .Font = New Font("Segoe UI", 9)
                                }
                                 ratingDropdown.Items.AddRange({"None", "⭐ Highest", "⭐ Lowest"})
                                 ratingDropdown.SelectedIndex = 0  ' Set "None" as default

                                 Dim sortBtn As New Button With {
                                    .Text = "SORT",
                                    .Location = New Point(470, 10),
                                    .Size = New Size(60, 30),
                                    .BackColor = Color.FromArgb(34, 197, 94),
                                    .ForeColor = Color.White,
                                    .FlatStyle = FlatStyle.Flat,
                                    .Font = New Font("Segoe UI", 9, FontStyle.Bold),
                                    .Cursor = Cursors.Hand
                                }
                                 sortBtn.FlatAppearance.BorderSize = 0

                                 Dim resetBtn As New Button With {
                                    .Text = "RESET",
                                    .Location = New Point(540, 10),
                                    .Size = New Size(60, 30),
                                    .BackColor = Color.FromArgb(239, 68, 68),
                                    .ForeColor = Color.White,
                                    .FlatStyle = FlatStyle.Flat,
                                    .Font = New Font("Segoe UI", 9, FontStyle.Bold),
                                    .Cursor = Cursors.Hand
                                }
                                 resetBtn.FlatAppearance.BorderSize = 0

                                 Dim searchLabel As New Label With {
                                       .Text = "Search Location:",
                                       .Font = New Font("Segoe UI", 10, FontStyle.Regular),
                                       .ForeColor = Color.White,
                                       .Location = New Point(Me.Width - 320, 15),
                                       .AutoSize = True
                                    }

                                 searchTextBox = New TextBox With {
                                       .Name = "searchTextBox",
                                       .Location = New Point(Me.Width - 200, 12),
                                       .Size = New Size(120, 25),
                                       .BackColor = Color.White,
                                       .Font = New Font("Segoe UI", 9)
                                    }

                                 Dim searchBtn As New Button With {
                                       .Text = "🔍",
                                       .Location = New Point(Me.Width - 70, 10),
                                       .Size = New Size(30, 30),
                                       .BackColor = Color.FromArgb(59, 130, 246),
                                       .ForeColor = Color.White,
                                       .FlatStyle = FlatStyle.Flat,
                                       .Font = New Font("Segoe UI", 12, FontStyle.Bold),
                                       .Cursor = Cursors.Hand
                                    }
                                 searchBtn.FlatAppearance.BorderSize = 0

                                 AddHandler sortBtn.Click, AddressOf SortButton_Click
                                 AddHandler resetBtn.Click, AddressOf ResetButton_Click
                                 AddHandler searchBtn.Click, AddressOf SearchButton_Click
                                 AddHandler searchTextBox.KeyDown, Sub(sender, e) If e.KeyCode = Keys.Enter Then SearchButton_Click(sender, e)

                                 filterPanel.Controls.AddRange({filterLabel, priceLabel, priceDropdown, ratingLabel, ratingDropdown, sortBtn, resetBtn, searchLabel, searchTextBox, searchBtn})
                                 Me.Controls.Add(filterPanel)
                                 filterPanel.BringToFront()

                                 SetupPackageCards()

                             End Sub
    End Sub

    Private Sub InitializePackages()
        packages = New List(Of TravelPackage)
        LoadPackagesFromDatabase()
        originalPackages = New List(Of TravelPackage)(packages)

        For Each package In packages
            Dim ratingData = GetPackageRating(package.Id)
            package.Rating = ratingData.rating
            package.ReviewCount = ratingData.count
        Next
    End Sub

    Private Sub LoadPackagesFromDatabase()
        Try
            packages.Clear()

            Using connection As New MySqlConnection(connectionString)
                connection.Open()

                Dim query = "
            SELECT 
                tp.PackageID as Id,
                tp.Title as Name,
                pl.Location as Destination,
                CONCAT(tp.Duration, ' day', IF(tp.Duration > 1, 's', '')) as Duration,
                tp.Price,
                pd.FlagSymbol as Flag,
                pd.IdealFor,
                tp.Description,
                pd.Inclusions,
                pd.Highlights,
                pd.ImageFileName,
                pa.AvailableSlots
            FROM TourPackages tp
            LEFT JOIN PackageLocations pl ON tp.PackageID = pl.PackageID
            LEFT JOIN PackageDetails pd ON tp.PackageID = pd.PackageID
            LEFT JOIN PackageAvailability pa ON tp.PackageID = pa.PackageID
            WHERE tp.PackageType = 'Domestic' AND tp.Status = 'Active' AND tp.IsActive = 1
            ORDER BY tp.CreatedAt DESC"

                Using cmd As New MySqlCommand(query, connection)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim package As New TravelPackage With {
                            .Id = Convert.ToInt32(reader("Id")),
                            .Name = If(reader("Name") IsNot DBNull.Value, reader("Name").ToString(), ""),
                            .Destination = If(reader("Destination") IsNot DBNull.Value, reader("Destination").ToString(), ""),
                            .Duration = If(reader("Duration") IsNot DBNull.Value, reader("Duration").ToString(), "1 day"),
                            .Price = If(reader("Price") IsNot DBNull.Value, Convert.ToDecimal(reader("Price")), 0),
                            .Flag = If(reader("Flag") IsNot DBNull.Value, reader("Flag").ToString(), "🏝️"),
                            .IdealFor = If(reader("IdealFor") IsNot DBNull.Value, reader("IdealFor").ToString(), "Everyone"),
                            .Description = If(reader("Description") IsNot DBNull.Value, reader("Description").ToString(), ""),
                            .ImageFileName = If(reader("ImageFileName") IsNot DBNull.Value, reader("ImageFileName").ToString(), ""),
                            .AvailableSlots = If(reader("AvailableSlots") IsNot DBNull.Value, Convert.ToInt32(reader("AvailableSlots")), 0),
                            .Inclusions = New List(Of String),
                            .Highlights = New List(Of String)
                        }

                            ' Parse inclusions if available
                            If reader("Inclusions") IsNot DBNull.Value AndAlso Not String.IsNullOrEmpty(reader("Inclusions").ToString()) Then
                                Try
                                    package.Inclusions = reader("Inclusions").ToString().Split({vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries).ToList()
                                Catch
                                    package.Inclusions = New List(Of String) From {reader("Inclusions").ToString()}
                                End Try
                            Else
                                package.Inclusions = New List(Of String)
                            End If

                            ' Parse highlights if available
                            If reader("Highlights") IsNot DBNull.Value AndAlso Not String.IsNullOrEmpty(reader("Highlights").ToString()) Then
                                Try
                                    package.Highlights = reader("Highlights").ToString().Split({vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries).ToList()
                                Catch
                                    package.Highlights = New List(Of String) From {reader("Highlights").ToString()}
                                End Try
                            Else
                                package.Highlights = New List(Of String)
                            End If

                            packages.Add(package)
                        End While
                    End Using
                End Using
            End Using

            ' Fallback packages with updated properties
            If packages.Count = 0 Then
                packages.AddRange({
                New TravelPackage With {
                    .Id = 1, .Name = "Palawan Paradise Explorer", .Destination = "El Nido, Coron, Puerto Princesa",
                    .Duration = "6 days, 5 nights", .Price = 28000, .Flag = "🏝️",
                    .IdealFor = "Beach lovers and adventure seekers",
                    .Description = "Discover the pristine beauty of Palawan's world-class beaches!",
                    .ImageFileName = "palawan.png", .AvailableSlots = 10,
                    .Inclusions = New List(Of String) From {"Round-trip flights from Manila", "5 nights accommodation", "Island hopping tours", "All meals included", "Professional tour guide", "Entrance fees"},
                    .Highlights = New List(Of String) From {"Underground River Tour", "Island Hopping in El Nido", "Coron Island Adventure", "Beach Activities", "Local Cultural Experience"}
                },
                New TravelPackage With {
                    .Id = 2, .Name = "Boracay Beach Getaway", .Destination = "Boracay Island",
                    .Duration = "4 days, 3 nights", .Price = 15000, .Flag = "🏖️",
                    .IdealFor = "Beach lovers and party enthusiasts",
                    .Description = "Experience the world-famous white sand beaches of Boracay!",
                    .ImageFileName = "boracay.png", .AvailableSlots = 10,
                    .Inclusions = New List(Of String) From {"Round-trip flights", "3 nights beachfront hotel", "Daily breakfast", "Island activities", "Airport transfers"},
                    .Highlights = New List(Of String) From {"White Beach relaxation", "Water sports activities", "Sunset sailing", "Beach parties", "Local seafood dining"}
                }
            })
            End If

        Catch ex As Exception
            MessageBox.Show($"Error loading packages from database: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Public Shared Sub RefreshPackageDisplay()
        ' Static method to refresh all open DomesticTravelForm instances
        For Each form As Form In Application.OpenForms
            If TypeOf form Is DomesticTravelForm Then
                DirectCast(form, DomesticTravelForms).RefreshPackages()
            End If
        Next
    End Sub

    Private Sub RefreshPackages()
        ' Clear existing packages and reload from database
        LoadPackagesFromDatabase()
        originalPackages = New List(Of TravelPackage)(packages)

        ' Clear and rebuild the display
        packagesFlowPanel.Controls.Clear()
        SetupPackageCards()
    End Sub

    Private Sub InitializeComponents()
        ' Form settings
        Me.Text = "LakbayPH Travel and Tours - International Packages"
        Me.Size = New Size(1400, 900)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(15, 23, 42)
        Me.Font = New Font("Segoe UI", 9, FontStyle.Regular)
        Me.WindowState = FormWindowState.Maximized
        Me.DoubleBuffered = True


        Try
            Dim bgImagePath As String = Path.Combine(Application.StartupPath, "loginbg.png")
            If Not File.Exists(bgImagePath) Then
                bgImagePath = Path.Combine(Application.StartupPath, "..\..\loginbg.png")
            End If

            If File.Exists(bgImagePath) Then
                Me.BackgroundImage = Image.FromFile(bgImagePath)
                Me.BackgroundImageLayout = ImageLayout.Stretch
            End If
        Catch
            ' Continue without background
        End Try

        ' Main panel with scroll
        mainPanel = New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.Transparent,
            .AutoScroll = True
        }

        ' Packages flow panel
        packagesFlowPanel = New Panel With {
            .Height = 450,
            .Dock = DockStyle.Fill,
            .BackColor = Color.Transparent,
            .AutoScroll = True,
            .Padding = New Padding(0, 25, 0, 250)
        }

        ' Services panel
        servicesPanel = New Panel With {
            .Height = 150,
            .Dock = DockStyle.Bottom,
            .BackColor = Color.FromArgb(200, 51, 65, 85),
            .Padding = New Padding(25)
        }

        mainPanel.Controls.AddRange({packagesFlowPanel, servicesPanel})
    End Sub

    Private Sub BackButton_Click(sender As Object, e As EventArgs)
        Dim pack As New TravelHomepageForm()
        pack.Show()
        ' Close current form
        Me.Close()
    End Sub

    Private Sub ListButton_Click(sender As Object, e As EventArgs)
        Dim bookinglist As New BookingListForm()
        bookinglist.Show()
    End Sub

    ' Filtering Methods
    Private Sub SearchButton_Click(sender As Object, e As EventArgs)
        Dim searchText As String = searchTextBox.Text.Trim().ToLower()

        If String.IsNullOrEmpty(searchText) Then
            packages = New List(Of TravelPackage)(originalPackages)
        Else
            packages = originalPackages.Where(Function(p) p.Destination.ToLower().Contains(searchText)).ToList()
        End If

        ' Update rating data for filtered packages
        For Each package In packages
            Dim ratingData = GetPackageRating(package.Id)
            package.Rating = ratingData.rating
            package.ReviewCount = ratingData.count
        Next

        DisplaySortedPackages(packages)
    End Sub

    Private Sub SortButton_Click(sender As Object, e As EventArgs)
        Dim priceDropdown As ComboBox = DirectCast(filterPanel.Controls("priceDropdown"), ComboBox)
        Dim ratingDropdown As ComboBox = DirectCast(filterPanel.Controls("ratingDropdown"), ComboBox)

        ' Start with all packages
        Dim sortedPackages As List(Of TravelPackage) = packages.ToList()

        ' Apply price sorting only if not "None"
        If priceDropdown.SelectedIndex > 0 Then
            If priceDropdown.SelectedIndex = 1 Then ' Lowest
                sortedPackages = sortedPackages.OrderBy(Function(p) p.Price).ToList()
            Else ' Highest (index 2)
                sortedPackages = sortedPackages.OrderByDescending(Function(p) p.Price).ToList()
            End If
        End If

        ' Apply rating sorting only if not "None"
        If ratingDropdown.SelectedIndex > 0 Then
            If ratingDropdown.SelectedIndex = 1 Then ' Highest
                sortedPackages = sortedPackages.OrderByDescending(Function(p) p.Rating).ToList()
            Else ' Lowest (index 2)
                sortedPackages = sortedPackages.OrderBy(Function(p) p.Rating).ToList()
            End If
        End If

        ' If both are "None", show original order (sorted by database query order)
        If priceDropdown.SelectedIndex = 0 AndAlso ratingDropdown.SelectedIndex = 0 Then
            sortedPackages = New List(Of TravelPackage)(originalPackages)
        End If

        DisplaySortedPackages(sortedPackages)
    End Sub

    Private Sub ResetButton_Click(sender As Object, e As EventArgs)
        Dim priceDropdown As ComboBox = DirectCast(filterPanel.Controls("priceDropdown"), ComboBox)
        Dim ratingDropdown As ComboBox = DirectCast(filterPanel.Controls("ratingDropdown"), ComboBox)

        ' Reset to "None" for both dropdowns
        priceDropdown.SelectedIndex = 0
        ratingDropdown.SelectedIndex = 0
        searchTextBox.Text = ""

        ' Restore original packages (database order)
        packages = New List(Of TravelPackage)(originalPackages)
        DisplaySortedPackages(packages)
    End Sub

    Private Sub DisplaySortedPackages(sortedPackages As List(Of TravelPackage))
        packagesFlowPanel.Controls.Clear()

        Dim panelWidth As Integer = packagesFlowPanel.Width
        Dim cardWidth As Integer = 400
        Dim cardHeight As Integer = 400
        Dim gapBetweenCards As Integer = 50
        Dim totalCardsWidth As Integer = cardWidth * 3 + gapBetweenCards * 2
        Dim startX As Integer = (panelWidth - totalCardsWidth) / 2

        For i As Integer = 0 To sortedPackages.Count - 1
            Dim row As Integer = i \ 3
            Dim col As Integer = i Mod 3

            Dim card As Panel = CreatePackageCard(sortedPackages(i))
            card.Location = New Point(startX + 10 + (col * (cardWidth + gapBetweenCards)), 180 + (row * (cardHeight + 35)))
            packagesFlowPanel.Controls.Add(card)
        Next

        Dim totalRows As Integer = Math.Ceiling(sortedPackages.Count / 3)
        Dim lastCardBottom As Integer = 140 + (totalRows * (cardHeight + 35))
        Dim spacer As New Panel With {
        .Size = New Size(1, 1),
        .Location = New Point(0, lastCardBottom + 120),
        .BackColor = Color.Transparent
    }
        packagesFlowPanel.Controls.Add(spacer)
    End Sub
    Private Sub SetupPackageCards()
        ' Calculate center positioning for 3 columns
        Dim panelWidth As Integer = packagesFlowPanel.Width
        Dim cardWidth As Integer = 400
        Dim cardHeight As Integer = 400
        Dim gapBetweenCards As Integer = 50
        Dim totalCardsWidth As Integer = cardWidth * 3 + gapBetweenCards * 2
        Dim startX As Integer = (panelWidth - totalCardsWidth) / 2

        For i As Integer = 0 To packages.Count - 1
            Dim row As Integer = i \ 3
            Dim col As Integer = i Mod 3

            Dim card As Panel = CreatePackageCard(packages(i))
            card.Location = New Point(startX + 10 + (col * (cardWidth + gapBetweenCards)), 140 + (row * (cardHeight + 35)))
            packagesFlowPanel.Controls.Add(card)
        Next

        Dim totalRows As Integer = Math.Ceiling(packages.Count / 3)
        Dim lastCardBottom As Integer = 140 + (totalRows * (cardHeight + 35))
        Dim spacer As New Panel With {
            .Size = New Size(1, 1),
            .Location = New Point(0, lastCardBottom + 120),
            .BackColor = Color.Transparent
        }
        packagesFlowPanel.Controls.Add(spacer)
    End Sub

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
        .Size = New Size(cardWidth - 30, 150),
        .Location = New Point(15, 15),
        .BackColor = Color.FromArgb(6, 41, 55)
    }

        ' Load the image with proper sizing
        If Not String.IsNullOrEmpty(package.ImageFileName) Then
            Try
                Dim imagePath As String = Path.Combine(Application.StartupPath, package.ImageFileName)
                If File.Exists(imagePath) Then
                    ' Create a PictureBox to display the image
                    Dim pictureBox As New PictureBox With {
                    .Size = New Size(cardWidth, 180),
                    .SizeMode = PictureBoxSizeMode.StretchImage, ' This will stretch to fill
                    .Dock = DockStyle.Fill,
                    .BackColor = Color.FromArgb(6, 41, 55)
                }

                    ' Load the image asynchronously to prevent UI freezing
                    Task.Run(Sub()
                                 Try
                                     Dim img = Image.FromFile(imagePath)
                                     pictureBox.Invoke(Sub() pictureBox.Image = img)
                                 Catch
                                     pictureBox.Invoke(Sub() CreateImagePlaceholder(imagePanel, package.Name))
                                 End Try
                             End Sub)

                    imagePanel.Controls.Add(pictureBox)
                Else
                    CreateImagePlaceholder(imagePanel, package.Name)
                End If
            Catch
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
        .ForeColor = Color.White,
        .BackColor = Color.Transparent,
        .Location = New Point(20, 230),
        .Size = New Size(360, 25)
    }

        ' Duration label
        Dim durationLabel As New Label With {
        .Text = $"⏱️ {package.Duration}",
        .Font = New Font("Segoe UI", 10, FontStyle.Bold),
        .ForeColor = Color.White,
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
        .ForeColor = Color.White,
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
        .ForeColor = Color.White,
        .BackColor = Color.Transparent,
        .Location = New Point(20, 305),
        .Size = New Size(100, 20)
    }

        ' View Details button
        Dim detailsBtn As New Button With {
        .Text = "View Full Itinerary",
        .Size = New Size(180, 40),
        .Location = New Point(25, 330),
        .BackColor = Color.FromArgb(25, 77, 98),
        .ForeColor = Color.White,
        .FlatStyle = FlatStyle.Flat,
        .Font = New Font("Segoe UI", 10, FontStyle.Bold),
        .Cursor = Cursors.Hand,
        .Tag = package
    }
        detailsBtn.FlatAppearance.BorderSize = 0

        ' Book Now button
        Dim bookBtn As New Button With {
        .Text = "Book Now",
        .Size = New Size(160, 40),
        .Location = New Point(215, 330),
        .BackColor = Color.FromArgb(34, 197, 94),
        .ForeColor = Color.White,
        .FlatStyle = FlatStyle.Flat,
        .Font = New Font("Segoe UI", 10, FontStyle.Bold),
        .Cursor = Cursors.Hand,
        .Tag = package
    }
        bookBtn.FlatAppearance.BorderSize = 0

        ' Add event handlers
        AddHandler detailsBtn.Click, AddressOf ViewDetails_Click
        AddHandler bookBtn.Click, AddressOf BookPackage_Click
        AddHandler card.Click, AddressOf Card_Click

        ' Add all controls to the card
        card.Controls.AddRange({
        nameLabel, destinationLabel, durationLabel,
        priceLabel, ratingLabel, reviewCountLabel,
        availabilityLabel, perPersonLabel,
        detailsBtn, bookBtn
    })

        Return card
    End Function
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

    Private Sub ImagePanel_Paint(sender As Object, e As PaintEventArgs)
        Dim panel As Panel = DirectCast(sender, Panel)
        Dim imageName As String = If(panel.Tag IsNot Nothing, panel.Tag.ToString(), "default.png")
        Dim rect As New Rectangle(0, 0, panel.Width, panel.Height)

        ' Try to load the image
        Try
            Dim imagePaths() As String = {
                Path.Combine("C:\Users\Mykyla\Source\Repos\proje\Resources", imageName),
                Path.GetFullPath(Path.Combine(Application.StartupPath, "..\..\Resources", imageName)),
                Path.Combine(Application.StartupPath, "Resources", imageName),
                Path.Combine(Application.StartupPath, "..\..\Resources", imageName),
                Path.Combine(Application.StartupPath, "PackageImages", imageName),
                Path.Combine(Application.StartupPath, "..\..\PackageImages", imageName),
                Path.Combine(Application.StartupPath, imageName),
                Path.Combine(Application.StartupPath, "..\..", imageName)
        }

            Dim imageFound As Boolean = False

            For Each imagePath In imagePaths
                If File.Exists(imagePath) Then
                    Using img As Image = Image.FromFile(imagePath)
                        ' Create rounded corners for top only
                        Using path As New Drawing2D.GraphicsPath()
                            Dim radius As Integer = 15
                            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90)
                            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90)
                            path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom)
                            path.CloseFigure()

                            e.Graphics.SetClip(path)
                            e.Graphics.DrawImage(img, rect)
                            e.Graphics.ResetClip()
                        End Using
                    End Using
                    imageFound = True
                    Exit For
                End If
            Next

            If Not imageFound Then
                ' If image not found, create placeholder
                CreateImagePlaceholder(panel, e.Graphics, rect)
            End If

        Catch ex As Exception
            ' If error occurs, create placeholder
            CreateImagePlaceholder(panel, e.Graphics, rect)
        End Try
    End Sub

    Private Sub CreateImagePlaceholder(panel As Panel, g As Graphics, rect As Rectangle)
        ' Background color
        Using brush As New SolidBrush(Color.FromArgb(6, 41, 55))
            g.FillRectangle(brush, rect)
        End Using

        ' Get package name from parent card
        Dim packageName As String = "Travel Package"
        If panel.Parent IsNot Nothing AndAlso panel.Parent.Tag IsNot Nothing Then
            Dim package = TryCast(panel.Parent.Tag, TravelPackage)
            If package IsNot Nothing Then
                packageName = package.Name
            End If
        End If

        ' Draw package name
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
    End Sub
    Private Function GetPackageRating(packageId As Integer) As (rating As Double, count As Integer)
        Try
            Using connection As New MySqlConnection(connectionString)
                connection.Open()

                ' Updated query using the new normalized structure
                Dim query As String = "SELECT IFNULL(AVG(r.Rating), 0) as AvgRating, 
                                          COUNT(r.ReviewID) as ReviewCount 
                                   FROM Reviews r
                                   INNER JOIN Bookings b ON r.BookingID = b.BookingID
                                   WHERE b.PackageID = @PackageID"

                Using command As New MySqlCommand(query, connection)
                    command.Parameters.AddWithValue("@PackageID", packageId)
                    Using reader As MySqlDataReader = command.ExecuteReader()
                        If reader.Read() Then
                            Dim avgRating As Double = Convert.ToDouble(reader("AvgRating"))
                            Dim reviewCount As Integer = Convert.ToInt32(reader("ReviewCount"))
                            Return (avgRating, reviewCount)
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error loading ratings for package {packageId}: {ex.Message}")
            Return (0, 0)
        End Try

        Return (0, 0)
    End Function
    Private Sub DestinationCard_Paint_New(sender As Object, e As PaintEventArgs)
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
        End Using
    End Sub

    Private Sub Card_Click(sender As Object, e As EventArgs)
        Dim card As Panel = DirectCast(sender, Panel)
        Dim package As TravelPackage = DirectCast(card.Tag, TravelPackage)
        ShowPackageDetailsPopup(package)
    End Sub

    Private Sub ViewDetails_Click(sender As Object, e As EventArgs)
        Dim btn As Button = DirectCast(sender, Button)
        Dim package As TravelPackage = DirectCast(btn.Tag, TravelPackage)
        ShowPackageDetailsPopup(package)
    End Sub

    Private Sub BookPackage_Click(sender As Object, e As EventArgs)
        Dim btn As Button = DirectCast(sender, Button)
        Dim package As TravelPackage = DirectCast(btn.Tag, TravelPackage)

        ' Check availability before proceeding
        If package.AvailableSlots <= 0 Then
            MessageBox.Show("Sorry, this package is fully booked.", "Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' Pass the selected package to BookingForm
        Dim book As New BookingForm(GlobalSession.UserID, GlobalSession.Username,
                       GlobalSession.Email, GlobalSession.Phone, package)
        book.Show()
    End Sub
    Private Sub ShowPackageDetailsPopup(package As TravelPackage)
        Dim detailsForm As New PackageDetailsForms(package)
        detailsForm.ShowDialog()
    End Sub

    Private Sub SetupServicesSection()
        Dim servicesTitle As New Label With {
            .Text = "🇵🇭 Additional Services & Benefits",
            .Font = New Font("Segoe UI", 18, FontStyle.Bold),
            .ForeColor = Color.FromArgb(34, 197, 94),
            .Location = New Point(0, 0),
            .AutoSize = True
        }

        Dim localLabel As New Label With {
            .Text = "🎒 Expert Local Guides | 🏨 Handpicked Accommodations | 🚐 Comfortable Transportation",
            .Font = New Font("Segoe UI", 12, FontStyle.Regular),
            .ForeColor = Color.White,
            .Location = New Point(0, 35),
            .Size = New Size(900, 25)
        }

        Dim supportLabel As New Label With {
            .Text = "📞 24/7 Local Support | 🍽️ Authentic Filipino Cuisine | 📸 Professional Photo Service",
            .Font = New Font("Segoe UI", 12, FontStyle.Regular),
            .ForeColor = Color.White,
            .Location = New Point(0, 60),
            .Size = New Size(900, 25)
        }

        Dim paymentLabel As New Label With {
            .Text = "💳 Flexible Payment: 50% Down Payment, Balance 15 days before departure",
            .Font = New Font("Segoe UI", 11, FontStyle.Bold),
            .ForeColor = Color.FromArgb(59, 130, 246),
            .Location = New Point(0, 90),
            .Size = New Size(800, 25)
        }

        Dim contactLabel As New Label With {
            .Text = "Contact Us: 📧 domestic@lakbayphtravel.com | 📱 +63 917 123 4567 | 🌐 www.lakbayphtravel.com",
            .Font = New Font("Segoe UI", 10, FontStyle.Regular),
            .ForeColor = Color.FromArgb(156, 163, 175),
            .Location = New Point(0, 120),
            .Size = New Size(1000, 25)
        }

        servicesPanel.Controls.AddRange({servicesTitle, localLabel, supportLabel, paymentLabel, contactLabel})
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        '
        'DomesticTravelForm
        '
        Me.ClientSize = New System.Drawing.Size(282, 253)
        Me.Name = "DomesticTravelForm"
        Me.ResumeLayout(False)

    End Sub

    Private Sub DomesticTravelForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class

Public Class PackageDetailsFormss
    Inherits Form

    Private package As TravelPackage
    Private mainPanel As Panel
    Private packages As Object
    Private connectionString As String

    Public Sub New(selectedPackage As TravelPackage)
        package = selectedPackage
        InitializeComponents()
        SetupUI()
    End Sub

    Private Sub InitializeComponents()
        ' Form settings
        Me.Text = $"{package.Name} - Complete Itinerary"
        Me.Size = New Size(800, 700)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.BackColor = Color.FromArgb(30, 41, 59)
        Me.Font = New Font("Segoe UI", 9, FontStyle.Regular)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False

        ' Main panel with scroll
        mainPanel = New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.FromArgb(30, 41, 59),
            .AutoScroll = True,
            .Padding = New Padding(25)
        }
        Me.Controls.Add(mainPanel)
    End Sub

    Private Sub SetupUI()
        ' Package details header
        Dim detailsHeader As New Label With {
            .Text = $"{package.Flag} {package.Name}",
            .Font = New Font("Segoe UI", 20, FontStyle.Bold),
            .ForeColor = Color.FromArgb(59, 130, 246),
            .Location = New Point(0, 0),
            .Size = New Size(750, 35)
        }

        ' Package info
        Dim infoLabel As New Label With {
            .Text = $"📍 {package.Destination} | ⏱️ {package.Duration} | 💰 ₱{package.Price:N0} per person",
            .Font = New Font("Segoe UI", 12, FontStyle.Regular),
            .ForeColor = Color.FromArgb(156, 163, 175),
            .Location = New Point(0, 45),
            .Size = New Size(750, 25)
        }

        ' Description
        Dim descriptionLabel As New Label With {
            .Text = package.Description,
            .Font = New Font("Segoe UI", 12, FontStyle.Italic),
            .ForeColor = Color.FromArgb(148, 163, 184),
            .Location = New Point(0, 75),
            .Size = New Size(750, 30)
        }

        Dim idealForLabel As New Label With {
            .Text = $"Ideal for: {package.IdealFor}",
            .Font = New Font("Segoe UI", 11, FontStyle.Regular),
            .ForeColor = Color.FromArgb(203, 213, 225),
            .Location = New Point(0, 105),
            .Size = New Size(750, 25)
        }

        ' Inclusions
        Dim inclusionsLabel As New Label With {
            .Text = "🎯 Package Inclusions:",
            .Font = New Font("Segoe UI", 14, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(0, 145),
            .AutoSize = True
        }

        Dim yPos As Integer = 175
        For Each inclusion In package.Inclusions
            Dim inclusionItem As New Label With {
                .Text = $"✈️ {inclusion}",
                .Font = New Font("Segoe UI", 10, FontStyle.Regular),
                .ForeColor = Color.FromArgb(203, 213, 225),
                .Location = New Point(30, yPos),
                .Size = New Size(700, 25)
            }
            mainPanel.Controls.Add(inclusionItem)
            yPos += 25
        Next

        ' Highlights
        Dim highlightsLabel As New Label With {
            .Text = "🌟 Trip Highlights:",
            .Font = New Font("Segoe UI", 14, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(0, yPos + 15),
            .AutoSize = True
        }
        mainPanel.Controls.Add(highlightsLabel)
        yPos += 45

        For Each highlight In package.Highlights
            Dim highlightItem As New Label With {
                .Text = $"⭐ {highlight}",
                .Font = New Font("Segoe UI", 10, FontStyle.Regular),
                .ForeColor = Color.FromArgb(34, 197, 94),
                .Location = New Point(30, yPos),
                .Size = New Size(700, 25)
            }
            mainPanel.Controls.Add(highlightItem)
            yPos += 25
        Next

        ' Close button
        Dim closeBtn As New Button With {
            .Text = "Close",
            .Size = New Size(100, 45),
            .Location = New Point(Me.ClientSize.Width - 120, Me.ClientSize.Height - 65), ' 20px margin from right and bottom
            .BackColor = Color.FromArgb(239, 68, 68),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI", 11, FontStyle.Bold),
            .Cursor = Cursors.Hand,
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Right ' Stays in bottom right when form resizes
}
        closeBtn.FlatAppearance.BorderSize = 0
        AddHandler closeBtn.Click, Sub() Me.Close()

        mainPanel.Controls.AddRange({detailsHeader, infoLabel, descriptionLabel, idealForLabel, inclusionsLabel, highlightsLabel, closeBtn})
    End Sub
    Private Sub LoadPackagesFromDatabase()
        Try
            packages.Clear()

            Using connection As New MySqlConnection(connectionString)
                connection.Open()

                ' Updated query using the new normalized structure
                Dim query = "
                    SELECT 
                        tp.PackageID as Id,
                        tp.Title as Name,
                        pl.Location as Destination,
                        CONCAT(tp.Duration, ' day', IF(tp.Duration > 1, 's', '')) as Duration,
                        tp.Price,
                        pd.FlagSymbol as Flag,
                        pd.IdealFor,
                        tp.Description,
                        pd.Inclusions,
                        pd.Highlights,
                        pd.ImageFileName,
                        pa.AvailableSlots  -- Get calculated available slots
                    FROM TourPackages tp
                    LEFT JOIN PackageLocations pl ON tp.PackageID = pl.PackageID
                    LEFT JOIN PackageDetails pd ON tp.PackageID = pd.PackageID
                    LEFT JOIN PackageAvailability pa ON tp.PackageID = pa.PackageID  -- Join with availability view
                    WHERE tp.PackageType = 'Domestic' AND tp.Status = 'Active' AND tp.IsActive = 1
                    ORDER BY tp.CreatedAt DESC"

                Using cmd As New MySqlCommand(query, connection)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim package As New TravelPackage With {
            .Id = Convert.ToInt32(reader("Id")),
            .Name = If(reader("Name") IsNot DBNull.Value, reader("Name").ToString(), ""),
            .Destination = If(reader("Destination") IsNot DBNull.Value, reader("Destination").ToString(), ""),
            .Duration = If(reader("Duration") IsNot DBNull.Value, reader("Duration").ToString(), "1 day"),
            .Price = If(reader("Price") IsNot DBNull.Value, Convert.ToDecimal(reader("Price")), 0),
            .Flag = If(reader("Flag") IsNot DBNull.Value, reader("Flag").ToString(), "🏝️"),
            .IdealFor = If(reader("IdealFor") IsNot DBNull.Value, reader("IdealFor").ToString(), "Everyone"),
            .Description = If(reader("Description") IsNot DBNull.Value, reader("Description").ToString(), ""),
            .ImageFileName = If(reader("ImageFileName") IsNot DBNull.Value, reader("ImageFileName").ToString(), ""),
            .AvailableSlots = If(reader("AvailableSlots") IsNot DBNull.Value, Convert.ToInt32(reader("AvailableSlots")), 0),  ' NEW
            .Inclusions = New List(Of String),
            .Highlights = New List(Of String)
        }

                            ' Parse inclusions if available
                            If reader("Inclusions") IsNot DBNull.Value AndAlso Not String.IsNullOrEmpty(reader("Inclusions").ToString()) Then
                                Try
                                    package.Inclusions = reader("Inclusions").ToString().Split({vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries).ToList()
                                Catch
                                    package.Inclusions = New List(Of String) From {reader("Inclusions").ToString()}
                                End Try
                            Else
                                package.Inclusions = New List(Of String)
                            End If

                            ' Parse highlights if available
                            If reader("Highlights") IsNot DBNull.Value AndAlso Not String.IsNullOrEmpty(reader("Highlights").ToString()) Then
                                Try
                                    package.Highlights = reader("Highlights").ToString().Split({vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries).ToList()
                                Catch
                                    package.Highlights = New List(Of String) From {reader("Highlights").ToString()}
                                End Try
                            Else
                                package.Highlights = New List(Of String)
                            End If

                            packages.Add(package)
                        End While
                    End Using
                End Using
            End Using

            ' Fallback packages with updated properties
            If packages.Count = 0 Then
                packages.AddRange({
            New TravelPackage With {
                .Id = 1, .Name = "Palawan Paradise Explorer", .Destination = "El Nido, Coron, Puerto Princesa",
                .Duration = "6 days, 5 nights", .Price = 28000, .Flag = "🏝️",
                .IdealFor = "Beach lovers and adventure seekers",
                .Description = "Discover the pristine beauty of Palawan's world-class beaches!",
                .ImageFileName = "palawan.png", .AvailableSlots = 25,
                .Inclusions = New List(Of String) From {"Round-trip flights from Manila", "5 nights accommodation", "Island hopping tours", "All meals included", "Professional tour guide", "Entrance fees"},
                .Highlights = New List(Of String) From {"Underground River Tour", "Island Hopping in El Nido", "Coron Island Adventure", "Beach Activities", "Local Cultural Experience"}
            },
            New TravelPackage With {
                .Id = 2, .Name = "Boracay Beach Getaway", .Destination = "Boracay Island",
                .Duration = "4 days, 3 nights", .Price = 15000, .Flag = "🏖️",
                .IdealFor = "Beach lovers and party enthusiasts",
                .Description = "Experience the world-famous white sand beaches of Boracay!",
                .ImageFileName = "boracay.png", .AvailableSlots = 30,
                .Inclusions = New List(Of String) From {"Round-trip flights", "3 nights beachfront hotel", "Daily breakfast", "Island activities", "Airport transfers"},
                .Highlights = New List(Of String) From {"White Beach relaxation", "Water sports activities", "Sunset sailing", "Beach parties", "Local seafood dining"}
            }
        })
            End If

        Catch ex As Exception
            MessageBox.Show($"Error loading packages from database: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Function CheckPackageAvailability(packageId As Integer, requestedSlots As Integer) As Boolean
        Try
            Using connection As New MySqlConnection(connectionString)
                connection.Open()

                Dim query As String = "SELECT AvailableSlots FROM PackageAvailability WHERE PackageID = @PackageID"

                Using command As New MySqlCommand(query, connection)
                    command.Parameters.AddWithValue("@PackageID", packageId)
                    Dim result = command.ExecuteScalar()

                    If result IsNot Nothing Then
                        Dim availableSlots As Integer = Convert.ToInt32(result)
                        Return availableSlots >= requestedSlots
                    End If
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error checking availability: {ex.Message}")
            Return False
        End Try

        Return False
    End Function

End Class

Public Class TravelPackagessss
    Public Property Id As Integer
    Public Property Name As String
    Public Property Destination As String
    Public Property Duration As String
    Public Property Price As Decimal
    Public Property Flag As String
    Public Property IdealFor As String
    Public Property Description As String
    Public Property Inclusions As List(Of String)
    Public Property Highlights As List(Of String)
    Public Property Rating As Double = 0.0
    Public Property ReviewCount As Integer = 0
    Public Property ImageFileName As String = ""
    Public Property AvailableSlots As Integer = 0  ' NEW PROPERTY
End Class

' Application entry point
Module Programessssssseses
    <STAThread>
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New DomesticTravelForm())
    End Sub
End Module