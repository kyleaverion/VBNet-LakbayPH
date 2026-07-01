Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient
Imports System.Linq
Public Class FreedivingPackagesForms

    Inherits Form

    Dim connectionString As String = "Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;"

    Public Property DatabaseID As Integer
    Private packages As List(Of FreedivingPackage)
    Private selectedPackage As FreedivingPackage = Nothing
    Private mainPanel As Panel
    Private packagesFlowPanel As Panel
    Private servicesPanel As Panel
    Private filterPanel As Panel

    Private package As FreedivingPackage

    Public Property Rating As Double = 0.0
    Public Property ReviewCount As Integer = 0

    Private originalPackages As List(Of FreedivingPackage)
    Private searchTextBox As TextBox

    Public Sub New()
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
                                 lblSubLogo.Text = "Dive Deep, Breathe Free - Premium Freediving Packages 🤿"
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
        packages = New List(Of FreedivingPackage)
        LoadPackagesFromDatabase()
        originalPackages = New List(Of FreedivingPackage)(packages)

        For Each package In packages
            Dim ratingData = GetPackageRating(package.Id)
            package.Rating = ratingData.rating
            package.ReviewCount = ratingData.count
        Next
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
        ' Hide current form
        Me.Hide()

        Dim pack As New TravelHomepageForm()
        pack.Show()
        ' Close current form
        Me.Close()
    End Sub

    Private Sub ListButton_Click(sender As Object, e As EventArgs)
        Dim bookinglist As New BookingListForm()
        bookinglist.Show()
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
            card.Location = New Point(startX + 10 + (col * (cardWidth + gapBetweenCards)), 180 + (row * (cardHeight + 35))) ' Changed from 140 to 180
            packagesFlowPanel.Controls.Add(card)
        Next

        Dim totalRows As Integer = Math.Ceiling(packages.Count / 3)
        Dim lastCardBottom As Integer = 180 + (totalRows * (cardHeight + 35)) ' Changed from 140 to 180
        Dim spacer As New Panel With {
        .Size = New Size(1, 1),
        .Location = New Point(0, lastCardBottom + 120),
        .BackColor = Color.Transparent
    }
        packagesFlowPanel.Controls.Add(spacer)
    End Sub

    Private Function CreatePackageCard(package As FreedivingPackage) As Panel
        Dim cardWidth As Integer = 400
        Dim cardHeight As Integer = 400

        ' Main card with white background and rounded corners
        Dim card As New Panel With {
        .Size = New Size(cardWidth, cardHeight),
        .BackColor = Color.FromArgb(200, 255, 255, 255),
        .Tag = package,
        .Cursor = Cursors.Hand
    }
        AddHandler card.Paint, AddressOf DestinationCard_Paint_New

        ' Image panel (top portion)
        Dim imagePanel As New Panel With {
        .Size = New Size(cardWidth - 30, 150),
        .Location = New Point(15, 15),
        .BackColor = Color.FromArgb(6, 41, 55)
    }

        ' Load the image if available
        If Not String.IsNullOrEmpty(package.ImageFileName) Then
            Try
                ' Try multiple possible locations for the image
                Dim imagePaths As New List(Of String) From {
                Path.Combine("C:\Users\Mykyla\Source\Repos\proje\Resources", package.ImageFileName),
                Path.GetFullPath(Path.Combine(Application.StartupPath, "..\..\Resources", package.ImageFileName)),
                Path.Combine(Application.StartupPath, "Resources", package.ImageFileName),
                Path.Combine(Application.StartupPath, "..\..\Resources", package.ImageFileName),
                Path.Combine(Application.StartupPath, "PackageImages", package.ImageFileName),
                Path.Combine(Application.StartupPath, "..\..\PackageImages", package.ImageFileName),
                Path.Combine(Application.StartupPath, package.ImageFileName),
                Path.Combine(Application.StartupPath, "..\..", package.ImageFileName)
            }

                Dim imagePath As String = imagePaths.FirstOrDefault(Function(p) File.Exists(p))

                If imagePath IsNot Nothing Then
                    ' Load image asynchronously to prevent UI freezing
                    Task.Run(Sub()
                                 Try
                                     Dim img = Image.FromFile(imagePath)
                                     imagePanel.Invoke(Sub()
                                                           Dim pictureBox As New PictureBox With {
                                        .Size = New Size(cardWidth, 180),
                                        .Image = img,
                                        .SizeMode = PictureBoxSizeMode.StretchImage,
                                        .Dock = DockStyle.Fill
                                    }
                                                           imagePanel.Controls.Add(pictureBox)
                                                       End Sub)
                                 Catch ex As Exception
                                     imagePanel.Invoke(Sub() CreateImagePlaceholder(imagePanel, package.Name))
                                 End Try
                             End Sub)
                Else
                    CreateImagePlaceholder(imagePanel, package.Name)
                End If
            Catch ex As Exception
                CreateImagePlaceholder(imagePanel, package.Name)
            End Try
        Else
            CreateImagePlaceholder(imagePanel, package.Name)
        End If

        card.Controls.Add(imagePanel)

        ' Package name label - position adjusted to be below image
        Dim nameLabel As New Label With {
        .Text = package.Name,
        .Font = New Font("Segoe UI", 16, FontStyle.Bold),
        .ForeColor = Color.FromArgb(6, 41, 55),
        .BackColor = Color.Transparent,
        .Location = New Point(20, 190), ' Below the image panel
        .Size = New Size(360, 45),
        .TextAlign = ContentAlignment.TopLeft
    }

        ' Destination label - position adjusted
        Dim destinationLabel As New Label With {
        .Text = $"📍 {package.Location}",
        .Font = New Font("Segoe UI", 10, FontStyle.Bold),
        .ForeColor = Color.FromArgb(6, 41, 55), ' Darker color for better contrast
        .BackColor = Color.Transparent,
        .Location = New Point(20, 230),
        .Size = New Size(360, 25)
    }

        ' Duration label - position adjusted
        Dim durationLabel As New Label With {
        .Text = $"⏱️ {package.Duration}",
        .Font = New Font("Segoe UI", 10, FontStyle.Bold),
        .ForeColor = Color.FromArgb(6, 41, 55),
        .BackColor = Color.Transparent,
        .Location = New Point(20, 255),
        .Size = New Size(200, 25)
    }

        ' Price label - position adjusted
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
        .ForeColor = Color.FromArgb(6, 41, 55),
        .BackColor = Color.Transparent,
        .Location = New Point(230, 295),
        .Size = New Size(150, 15)
    }

        ' Per person label
        Dim perPersonLabel As New Label With {
        .Text = "per person",
        .Font = New Font("Segoe UI", 9, FontStyle.Bold),
        .ForeColor = Color.FromArgb(6, 41, 55),
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
        perPersonLabel, detailsBtn, bookBtn
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

            ' Draw dive icon
            Using iconFont As New Font("Segoe UI Emoji", 24, FontStyle.Bold)
                Using iconBrush As New SolidBrush(Color.White)
                    g.DrawString("🤿", iconFont, iconBrush, 20, panel.Height - 50)
                End Using
            End Using
        End Using
    End Sub

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
            e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            e.Graphics.FillPath(New SolidBrush(panel.BackColor), path)
        End Using
    End Sub

    Private Sub Card_Click(sender As Object, e As EventArgs)
        Dim card As Panel = DirectCast(sender, Panel)
        Dim package As FreedivingPackage = DirectCast(card.Tag, FreedivingPackage)
        ShowPackageDetailsPopup(package)
    End Sub

    Private Sub ViewDetails_Click(sender As Object, e As EventArgs)
        Dim btn As Button = DirectCast(sender, Button)
        Dim package As FreedivingPackage = DirectCast(btn.Tag, FreedivingPackage)
        ShowPackageDetailsPopup(package)
    End Sub

    Private Sub BookPackage_Click(sender As Object, e As EventArgs)
        Dim btn As Button = DirectCast(sender, Button)
        Dim package As FreedivingPackage = DirectCast(btn.Tag, FreedivingPackage)

        ' Pass the selected package to BookingForm
        Dim book As New BookingForm(GlobalSession.UserID, GlobalSession.Username,
                           GlobalSession.Email, GlobalSession.Phone, package)
        book.Show()
    End Sub

    Private Sub ShowPackageDetailsPopup(package As FreedivingPackage)
        Try
            If package Is Nothing Then
                MessageBox.Show("Package information is not available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Ensure package has initialized properties
            If package.Inclusions Is Nothing Then
                package.Inclusions = New List(Of String)
            End If

            If package.Schedule Is Nothing Then
                package.Schedule = New List(Of String)
            End If

            ' Ensure basic properties are not null
            If String.IsNullOrEmpty(package.Name) Then
                package.Name = "Freediving Package"
            End If

            If String.IsNullOrEmpty(package.Location) Then
                package.Location = "Location TBA"
            End If

            If String.IsNullOrEmpty(package.Duration) Then
                package.Duration = "1 day"
            End If

            If String.IsNullOrEmpty(package.Icon) Then
                package.Icon = "🤿"
            End If

            If String.IsNullOrEmpty(package.IdealFor) Then
                package.IdealFor = "Everyone"
            End If

            If String.IsNullOrEmpty(package.Description) Then
                package.Description = "Amazing freediving experience"
            End If

            Dim detailsForm As New FreedivingDetailsForm(package)
            detailsForm.ShowDialog()
        Catch ex As Exception
            MessageBox.Show($"Error creating details form: {ex.Message}{vbCrLf}Stack Trace: {ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Updated FreedivingDetailsForm Constructor


    Private Sub SetupServicesSection()
        Dim servicesTitle As New Label With {
            .Text = "🎁 Additional Services & Add-ons",
            .Font = New Font("Segoe UI", 18, FontStyle.Bold),
            .ForeColor = Color.FromArgb(34, 197, 94),
            .Location = New Point(0, 0),
            .AutoSize = True
        }

        Dim addonsLabel As New Label With {
            .Text = "📸 Underwater Photography (₱1,500) | 🧜‍♀️ Mermaid Tail Dive (₱1,000) | 🤿 Extra Boat Dive (₱1,000)",
            .Font = New Font("Segoe UI", 12, FontStyle.Regular),
            .ForeColor = Color.White,
            .Location = New Point(0, 35),
            .Size = New Size(1000, 25)
        }

        Dim servicesLabel As New Label With {
            .Text = "🚐 Transportation from Manila (₱2,500) | 🐕 Pet-Friendly (₱500/night) | 🤿 Snorkeling Gear (₱300)",
            .Font = New Font("Segoe UI", 12, FontStyle.Regular),
            .ForeColor = Color.White,
            .Location = New Point(0, 60),
            .Size = New Size(1000, 25)
        }

        Dim discountLabel As New Label With {
            .Text = "💰 Group Discounts: 5% off (5-9 pax) | 10% off (10+ pax) | Senior/PWD: Additional 10% off",
            .Font = New Font("Segoe UI", 11, FontStyle.Bold),
            .ForeColor = Color.FromArgb(59, 130, 246),
            .Location = New Point(0, 90),
            .Size = New Size(1000, 25)
        }

        Dim contactLabel As New Label With {
            .Text = "Contact Us: 📧 info@lakbayphfreediving.com | 📱 +63 917 123 4567 | 🌐 www.lakbayphfreediving.com",
            .Font = New Font("Segoe UI", 10, FontStyle.Regular),
            .ForeColor = Color.FromArgb(156, 163, 175),
            .Location = New Point(0, 120),
            .Size = New Size(1000, 25)
        }

        servicesPanel.Controls.AddRange({servicesTitle, addonsLabel, servicesLabel, discountLabel, contactLabel})
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        '
        'FreedivingPackagesForm
        '
        Me.ClientSize = New System.Drawing.Size(282, 253)
        Me.Name = "FreedivingPackagesForm"
        Me.ResumeLayout(False)

    End Sub

    ' Filtering Methods

    Private Sub SearchButton_Click(sender As Object, e As EventArgs)
        Dim searchText As String = searchTextBox.Text.Trim().ToLower()

        If String.IsNullOrEmpty(searchText) Then
            packages = New List(Of FreedivingPackage)(originalPackages)
        Else
            packages = originalPackages.Where(Function(p) p.Location.ToLower().Contains(searchText)).ToList()
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
        Dim sortedPackages As List(Of FreedivingPackage) = packages.ToList()

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
            sortedPackages = New List(Of FreedivingPackage)(originalPackages)
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
        packages = New List(Of FreedivingPackage)(originalPackages)
        DisplaySortedPackages(packages)
    End Sub

    Private Sub DisplaySortedPackages(sortedPackages As List(Of FreedivingPackage))
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

            ' Get the original package to ensure we have correct rating data
            Dim originalPackage = packages.FirstOrDefault(Function(p) p.Id = sortedPackages(i).Id)
            If originalPackage IsNot Nothing Then
                sortedPackages(i).Rating = originalPackage.Rating
                sortedPackages(i).ReviewCount = originalPackage.ReviewCount
            End If

            Dim card As Panel = CreatePackageCard(sortedPackages(i))
            card.Location = New Point(startX + 10 + (col * (cardWidth + gapBetweenCards)), 180 + (row * (cardHeight + 35)))
            packagesFlowPanel.Controls.Add(card)
        Next

        Dim totalRows As Integer = Math.Ceiling(sortedPackages.Count / 3)
        Dim lastCardBottom As Integer = 180 + (totalRows * (cardHeight + 35))
        Dim spacer As New Panel With {
        .Size = New Size(1, 1),
        .Location = New Point(0, lastCardBottom + 120),
        .BackColor = Color.Transparent
    }
        packagesFlowPanel.Controls.Add(spacer)
    End Sub

    Private Function GetPackageRating(packageId As Integer) As (rating As Double, count As Integer)
        Try
            Using connection As New MySqlConnection(connectionString)
                connection.Open()

                ' Fixed query - directly filter by packageID in bookings table
                Dim query As String = "SELECT IFNULL(AVG(r.rating), 0) as AvgRating, 
                                          COUNT(r.reviewID) as ReviewCount 
                                   FROM reviews r
                                   INNER JOIN bookings b ON r.bookingID = b.bookingID
                                   WHERE b.packageID = @PackageID"

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

    Public Shared Sub RefreshPackageDisplay()
        ' Static method to refresh all open FreedivingPackagesForm instances
        For Each form As Form In Application.OpenForms
            If TypeOf form Is FreedivingPackagesForms Then
                DirectCast(form, FreedivingPackagesForms).RefreshPackages()
            End If
        Next
    End Sub

    Private Sub RefreshPackages()
        ' Clear existing packages and reload from database
        LoadPackagesFromDatabase()
        originalPackages = New List(Of FreedivingPackage)(packages)

        ' Clear and rebuild the display
        packagesFlowPanel.Controls.Clear()
        SetupPackageCards()
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
                pl.Location,
                CONCAT(tp.Duration, ' day', IF(tp.Duration > 1, 's', '')) as Duration,
                tp.Price,
                pd.FlagSymbol as Icon,
                pd.IdealFor,
                tp.Description,
                pd.Inclusions,
                pd.Highlights,
                pd.ImageFileName
            FROM TourPackages tp
            LEFT JOIN PackageLocations pl ON tp.PackageID = pl.PackageID
            LEFT JOIN PackageDetails pd ON tp.PackageID = pd.PackageID
            WHERE tp.PackageType = 'Freediving' AND tp.Status = 'Active'
            ORDER BY tp.CreatedAt DESC"

                Using cmd As New MySqlCommand(query, connection)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim package As New FreedivingPackage With {
                            .Id = Convert.ToInt32(reader("Id")),
                            .Name = If(reader("Name") IsNot DBNull.Value, reader("Name").ToString(), ""),
                            .Location = If(reader("Location") IsNot DBNull.Value, reader("Location").ToString(), ""),
                            .Duration = If(reader("Duration") IsNot DBNull.Value, reader("Duration").ToString(), "1 day"),
                            .Price = If(reader("Price") IsNot DBNull.Value, Convert.ToDecimal(reader("Price")), 0),
                            .Icon = If(reader("Icon") IsNot DBNull.Value, reader("Icon").ToString(), "🤿"),
                            .IdealFor = If(reader("IdealFor") IsNot DBNull.Value, reader("IdealFor").ToString(), "Everyone"),
                            .Description = If(reader("Description") IsNot DBNull.Value, reader("Description").ToString(), ""),
                            .ImageFileName = If(reader("ImageFileName") IsNot DBNull.Value, reader("ImageFileName").ToString(), ""),
                            .Inclusions = New List(Of String),
                            .Schedule = New List(Of String)
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

                            ' Parse highlights as schedule if available
                            If reader("Highlights") IsNot DBNull.Value AndAlso Not String.IsNullOrEmpty(reader("Highlights").ToString()) Then
                                Try
                                    package.Schedule = reader("Highlights").ToString().Split({vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries).ToList()
                                Catch
                                    package.Schedule = New List(Of String) From {reader("Highlights").ToString()}
                                End Try
                            Else
                                package.Schedule = New List(Of String)
                            End If

                            packages.Add(package)
                        End While
                    End Using
                End Using
            End Using

            ' If no packages loaded from database, add fallback packages
            If packages.Count = 0 Then
                packages.Add(New FreedivingPackage With {
            .Id = 1,
            .Name = "Basic Freediving Package",
            .Location = "Batangas",
            .Duration = "1 day",
            .Price = 2500,
            .Icon = "🤿",
            .IdealFor = "Beginners",
            .Description = "Perfect introduction to freediving",
            .Inclusions = New List(Of String) From {"Training", "Equipment", "Lunch"},
            .Schedule = New List(Of String) From {"Morning briefing", "Training session", "Practice dive"},
            .Rating = 0.0,
            .ReviewCount = 0
        })
            End If

            ' Load ratings for each package
            For Each package In packages
                Try
                    Dim ratingData = GetPackageRating(package.Id)
                    package.Rating = ratingData.rating
                    package.ReviewCount = ratingData.count
                Catch ex As Exception
                    package.Rating = 0.0
                    package.ReviewCount = 0
                End Try
            Next

        Catch ex As Exception
            MessageBox.Show($"Error loading packages from database: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

            ' Add fallback package if database fails
            If packages.Count = 0 Then
                packages.Add(New FreedivingPackage With {
            .Id = 1,
            .Name = "Basic Freediving Package",
            .Location = "Batangas",
            .Duration = "1 day",
            .Price = 2500,
            .Icon = "🤿",
            .IdealFor = "Beginners",
            .Description = "Perfect introduction to freediving",
            .Inclusions = New List(Of String) From {"Training", "Equipment", "Lunch"},
            .Schedule = New List(Of String) From {"Morning briefing", "Training session", "Practice dive"},
            .Rating = 0.0,
            .ReviewCount = 0
        })
            End If
        End Try
    End Sub
    Private Sub FreedivingPackagesForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class

Public Class FreedivingDetailsFormes
    Inherits Form

    Private package As FreedivingPackage
    Private mainPanel As Panel
    Private contentPanel As Panel
    Private closeBtn As Button

    Public Sub New(selectedPackage As FreedivingPackage)
        If selectedPackage Is Nothing Then
            MessageBox.Show("Package information is not available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.Close()
            Return
        End If

        package = selectedPackage
        InitializeComponents()
        SetupUI()
    End Sub

    Private Sub InitializeComponents()
        If package Is Nothing Then
            Return
        End If

        ' Form settings
        Me.Text = $"{If(package.Name, "Package")} - Complete Details"
        Me.Size = New Size(850, 750)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.BackColor = Color.FromArgb(30, 41, 59)
        Me.Font = New Font("Segoe UI", 9, FontStyle.Regular)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False

        ' Main panel that holds everything
        mainPanel = New Panel With {
        .Dock = DockStyle.Fill,
        .BackColor = Color.FromArgb(30, 41, 59),
        .AutoScroll = True
    }
        Me.Controls.Add(mainPanel)

        ' Content panel that holds all the details
        contentPanel = New Panel With {
        .Dock = DockStyle.Top,
        .BackColor = Color.FromArgb(30, 41, 59),
        .AutoSize = True,
        .AutoSizeMode = AutoSizeMode.GrowAndShrink,
        .Padding = New Padding(25),
        .Margin = New Padding(0, 0, 0, 80)
    }
        mainPanel.Controls.Add(contentPanel)

        ' Close button
        closeBtn = New Button With {
        .Text = "Close",
        .Size = New Size(100, 45),
        .BackColor = Color.FromArgb(239, 68, 68),
        .ForeColor = Color.White,
        .FlatStyle = FlatStyle.Flat,
        .Font = New Font("Segoe UI", 11, FontStyle.Bold),
        .Cursor = Cursors.Hand,
        .Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
    }
        closeBtn.FlatAppearance.BorderSize = 0
        AddHandler closeBtn.Click, Sub() Me.Close()

        closeBtn.Location = New Point(Me.ClientSize.Width - 120, Me.ClientSize.Height - 65)
        Me.Controls.Add(closeBtn)
        closeBtn.BringToFront()
    End Sub

    ' Change the SetupUI method from Private to Protected or Public
    Protected Sub SetupUI()
        If package Is Nothing Then
            MessageBox.Show("Package information is not available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.Close()
            Return
        End If

        ' Ensure properties have default values if null
        Dim packageName As String = If(String.IsNullOrEmpty(package.Name), "Unknown Package", package.Name)
        Dim packageIcon As String = If(String.IsNullOrEmpty(package.Icon), "🤿", package.Icon)
        Dim packageLocation As String = If(String.IsNullOrEmpty(package.Location), "Location TBA", package.Location)
        Dim packageDuration As String = If(String.IsNullOrEmpty(package.Duration), "Duration TBA", package.Duration)
        Dim packageDescription As String = If(String.IsNullOrEmpty(package.Description), "Description coming soon", package.Description)
        Dim packageIdealFor As String = If(String.IsNullOrEmpty(package.IdealFor), "Everyone", package.IdealFor)

        Dim detailsHeader As New Label With {
    .Text = $"{packageIcon} {packageName}",
    .Font = New Font("Segoe UI", 20, FontStyle.Bold),
    .ForeColor = Color.FromArgb(59, 130, 246),
    .Location = New Point(0, 0),
    .Size = New Size(750, 35)
}

        Dim priceText As String = $"₱{package.Price:N0} per person"
        Dim infoLabel As New Label With {
    .Text = $"📍 {packageLocation} | ⏱️ {packageDuration} | 💰 {priceText}",
    .Font = New Font("Segoe UI", 12, FontStyle.Regular),
    .ForeColor = Color.FromArgb(156, 163, 175),
    .Location = New Point(0, 45),
    .Size = New Size(750, 25)
}

        Dim descriptionLabel As New Label With {
    .Text = packageDescription,
    .Font = New Font("Segoe UI", 12, FontStyle.Italic),
    .ForeColor = Color.FromArgb(148, 163, 184),
    .Location = New Point(0, 75),
    .Size = New Size(750, 30)
}

        Dim idealForLabel As New Label With {
    .Text = $"Ideal for: {packageIdealFor}",
    .Font = New Font("Segoe UI", 11, FontStyle.Regular),
    .ForeColor = Color.FromArgb(203, 213, 225),
    .Location = New Point(0, 105),
    .Size = New Size(750, 25)
}

        Dim yPos As Integer = 145

        ' Show inclusions if available
        If package.Inclusions IsNot Nothing AndAlso package.Inclusions.Count > 0 Then
            Dim inclusionsLabel As New Label With {
        .Text = "🎯 Package Inclusions:",
        .Font = New Font("Segoe UI", 14, FontStyle.Bold),
        .ForeColor = Color.White,
        .Location = New Point(0, yPos),
        .AutoSize = True
    }
            contentPanel.Controls.Add(inclusionsLabel)
            yPos += 30

            For Each inclusion In package.Inclusions
                If Not String.IsNullOrWhiteSpace(inclusion) Then
                    Dim inclusionItem As New Label With {
                .Text = $"✓ {inclusion.Trim()}",
                .Font = New Font("Segoe UI", 10, FontStyle.Regular),
                .ForeColor = Color.FromArgb(203, 213, 225),
                .Location = New Point(30, yPos),
                .Size = New Size(700, 25)
            }
                    contentPanel.Controls.Add(inclusionItem)
                    yPos += 25
                End If
            Next
            yPos += 15
        Else
            Dim inclusionsLabel As New Label With {
        .Text = "🎯 Package Inclusions:",
        .Font = New Font("Segoe UI", 14, FontStyle.Bold),
        .ForeColor = Color.White,
        .Location = New Point(0, yPos),
        .AutoSize = True
    }
            contentPanel.Controls.Add(inclusionsLabel)
            yPos += 30

            Dim defaultInclusion As New Label With {
        .Text = "✓ Standard package inclusions apply",
        .Font = New Font("Segoe UI", 10, FontStyle.Regular),
        .ForeColor = Color.FromArgb(203, 213, 225),
        .Location = New Point(30, yPos),
        .Size = New Size(700, 25)
    }
            contentPanel.Controls.Add(defaultInclusion)
            yPos += 40
        End If

        ' Show highlights/schedule if available
        If package.Schedule IsNot Nothing AndAlso package.Schedule.Count > 0 Then
            Dim scheduleLabel As New Label With {
        .Text = "📅 Package Highlights:",
        .Font = New Font("Segoe UI", 14, FontStyle.Bold),
        .ForeColor = Color.White,
        .Location = New Point(0, yPos),
        .AutoSize = True
    }
            contentPanel.Controls.Add(scheduleLabel)
            yPos += 30

            For Each scheduleItem In package.Schedule
                If Not String.IsNullOrWhiteSpace(scheduleItem) Then
                    Dim scheduleItemLabel As New Label With {
                .Text = $"⭐ {scheduleItem.Trim()}",
                .Font = New Font("Segoe UI", 10, FontStyle.Regular),
                .ForeColor = Color.FromArgb(34, 197, 94),
                .Location = New Point(30, yPos),
                .Size = New Size(700, 25)
            }
                    contentPanel.Controls.Add(scheduleItemLabel)
                    yPos += 25
                End If
            Next
            yPos += 15
        Else
            Dim scheduleLabel As New Label With {
        .Text = "📅 Package Highlights:",
        .Font = New Font("Segoe UI", 14, FontStyle.Bold),
        .ForeColor = Color.White,
        .Location = New Point(0, yPos),
        .AutoSize = True
    }
            contentPanel.Controls.Add(scheduleLabel)
            yPos += 30

            Dim defaultSchedule As New Label With {
        .Text = "⭐ Detailed itinerary will be provided upon booking",
        .Font = New Font("Segoe UI", 10, FontStyle.Regular),
        .ForeColor = Color.FromArgb(34, 197, 94),
        .Location = New Point(30, yPos),
        .Size = New Size(700, 25)
    }
            contentPanel.Controls.Add(defaultSchedule)
            yPos += 40
        End If

        ' Add-ons section
        Dim addonsLabel As New Label With {
    .Text = "🎁 Available Add-ons:",
    .Font = New Font("Segoe UI", 14, FontStyle.Bold),
    .ForeColor = Color.White,
    .Location = New Point(0, yPos),
    .AutoSize = True
}
        contentPanel.Controls.Add(addonsLabel)
        yPos += 30

        Dim addons As List(Of String) = New List(Of String) From {
    "Underwater Photography: ₱1,500 per session",
    "Mermaid Tail Dive: ₱1,000 per dive",
    "Extra Boat Dive: ₱1,000 per dive",
    "Transportation from Manila: ₱2,500 round-trip",
    "Pet Fee (Small-Medium): ₱500 per night"
}

        For Each addon In addons
            Dim addonItem As New Label With {
        .Text = $"+ {addon}",
        .Font = New Font("Segoe UI", 10, FontStyle.Regular),
        .ForeColor = Color.FromArgb(156, 163, 175),
        .Location = New Point(30, yPos),
        .Size = New Size(700, 25)
    }
            contentPanel.Controls.Add(addonItem)
            yPos += 25
        Next

        contentPanel.Height = yPos + 50

        contentPanel.Controls.AddRange({detailsHeader, infoLabel, descriptionLabel, idealForLabel})
    End Sub
End Class

Public Class FreedivingPackagesss
    Public Property Id As Integer
    Public Property Name As String
    Public Property Duration As String
    Public Property Price As Decimal
    Public Property Icon As String
    Public Property IdealFor As String
    Public Property Description As String
    Public Property Inclusions As List(Of String)
    Public Property Highlights As List(Of String)
    Public Property Schedule As List(Of String)
    Public Property Location As String
    Public Property Rating As Double = 0.0 ' Add this line
    Public Property ReviewCount As Integer = 0 ' Add this line
    Public Property ImageFileName As String = ""  ' Add this line
End Class

' Application entry point
Module Programesreseeses
    <STAThread>
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New FreedivingPackagesForm())
    End Sub
End Module