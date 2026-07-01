Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Windows.Forms
Imports projectv2.MainForm

Public Class TravelHomepageForm
    Inherits Form

    Private mainPanel As Panel
    Private headerPanel As Panel
    Private destinationsPanel As Panel
    Private featuresPanel As Panel
    Private footerPanel As Panel

    Private WithEvents gradientTimer As Timer
    Private gradientOffset As Single = 0
    Private gradientPanel As Panel

    Private user As MainForm.UserInfo
    Private CurrentUser As Integer

    Public Sub New()
        InitializeComponent()
        SetupUI()
    End Sub

    Public Sub New(user As MainForm.UserInfo)
        Me.user = user
        InitializeComponent()
        SetupUI()
    End Sub

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(TravelHomepageForm))
        Me.SuspendLayout()
        '
        'TravelHomepageForm
        '
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(248, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(1382, 853)
        Me.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MinimumSize = New Size(1000, 600)
        Me.Name = "TravelHomepageForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Lakbay PH Travel & Tours - Discover Amazing Destinations"
        Me.FormBorderStyle = FormBorderStyle.Sizable
        Me.ResumeLayout(False)
        Me.DoubleBuffered = True

    End Sub

    Private Sub SetupUI()
        ' Set form background with image
        Try
            ' Try output directory first, then source directory
            Dim bgImagePath As String = Path.Combine(Application.StartupPath, "loginbg.png")
            If Not File.Exists(bgImagePath) Then
                bgImagePath = Path.Combine(Application.StartupPath, "..\..\loginbg.png")
            End If

            If File.Exists(bgImagePath) Then
                Me.BackgroundImage = Image.FromFile(bgImagePath)
                Me.BackgroundImageLayout = ImageLayout.Stretch
            Else
                Me.BackColor = Color.FromArgb(45, 85, 95)
            End If
        Catch
            Me.BackColor = Color.FromArgb(45, 85, 95)
        End Try

        ' Main container with proper scrolling
        mainPanel = New Panel With {
            .Dock = DockStyle.Fill,
            .AutoScroll = True,
            .BackColor = Color.Transparent
        }
        ' Set a larger height to ensure scrolling works
        mainPanel.AutoScrollMinSize = New Size(0, 1200)
        Me.Controls.Add(mainPanel)

        CreateTopNavigation()
        CreateWhyChooseSection()
        CreateDestinationsSection()
        CreateFooter()
    End Sub

    Private Sub CreateTopNavigation()
        ' Top navigation panel with background
        Dim pnlTopNav As New Panel()
        pnlTopNav.Size = New Size(Me.Width, 70)
        pnlTopNav.Location = New Point(0, 0)
        pnlTopNav.BackColor = Color.White
        pnlTopNav.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        mainPanel.Controls.Add(pnlTopNav)

        ' Logo
        Dim logo As New PictureBox()
        logo.Size = New Size(40, 40)
        logo.Location = New Point(50, 15)
        Try
            Dim logoPath As String = Path.Combine(Application.StartupPath, "logo.png")
            If Not File.Exists(logoPath) Then
                logoPath = Path.Combine(Application.StartupPath, "..\..\logo.png")
            End If
            If File.Exists(logoPath) Then
                logo.Image = Image.FromFile(logoPath)
                logo.SizeMode = PictureBoxSizeMode.StretchImage
            Else
                logo.BackColor = Color.White
            End If
        Catch
            logo.BackColor = Color.White
        End Try
        pnlTopNav.Controls.Add(logo)

        ' User welcome label (if user is logged in)
        If user IsNot Nothing Then
            Dim welcomeLabel As New Label With {
                .Text = $"Welcome, {user.Username}!",
                .Font = New Font("Arial", 20, FontStyle.Regular),
                .ForeColor = Color.FromArgb(6, 41, 55),
                .BackColor = Color.Transparent,
                .AutoSize = True,
                .Location = New Point(100, 15)
            }
            pnlTopNav.Controls.Add(welcomeLabel)
        End If

        ' Navigation buttons
        Dim btnPackages As New Button()
        btnPackages.Text = "Packages"
        btnPackages.Font = New Font("Arial", 11, FontStyle.Regular)
        btnPackages.ForeColor = Color.FromArgb(6, 41, 55)
        btnPackages.BackColor = Color.Transparent
        btnPackages.FlatStyle = FlatStyle.Flat
        btnPackages.FlatAppearance.BorderSize = 0
        btnPackages.Location = New Point(pnlTopNav.Width - 500, 23.5)
        btnPackages.Size = New Size(100, 30)
        btnPackages.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnPackages.Cursor = Cursors.Hand
        AddHandler btnPackages.Click, AddressOf PackagesButton_Click
        pnlTopNav.Controls.Add(btnPackages)

        Dim btnAboutUs As New Button()
        btnAboutUs.Text = "About Us"
        btnAboutUs.Font = New Font("Arial", 11, FontStyle.Regular)
        btnAboutUs.ForeColor = Color.FromArgb(6, 41, 55)
        btnAboutUs.BackColor = Color.Transparent
        btnAboutUs.FlatStyle = FlatStyle.Flat
        btnAboutUs.FlatAppearance.BorderSize = 0
        btnAboutUs.Location = New Point(pnlTopNav.Width - 380, 23.5)
        btnAboutUs.Size = New Size(80, 30)
        btnAboutUs.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnAboutUs.Cursor = Cursors.Hand
        AddHandler btnAboutUs.Click, AddressOf AboutUsButton_Click
        pnlTopNav.Controls.Add(btnAboutUs)

        ' Booking List Button
        Dim listButton As New Button With {
            .Text = "📋",
            .Size = New Size(40, 40),
            .Location = New Point(pnlTopNav.Width - 280, 15),
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

        ' Profile button
        Dim profileButton As New Button With {
            .Text = "👤",
            .Size = New Size(40, 40),
            .Location = New Point(pnlTopNav.Width - 200, 15),
            .BackColor = Color.FromArgb(6, 41, 55),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .Cursor = Cursors.Hand,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right
        }
        profileButton.FlatAppearance.BorderSize = 0
        AddHandler profileButton.Click, AddressOf ProfileButton_Click
        pnlTopNav.Controls.Add(profileButton)

        ' Logout button
        Dim logoutButton As New Button With {
            .Text = "LogOut",
            .Size = New Size(70, 30),
            .Location = New Point(pnlTopNav.Width - 130, 20),
            .BackColor = Color.FromArgb(6, 41, 55),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Arial", 10, FontStyle.Bold),
            .Cursor = Cursors.Hand,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right
        }
        logoutButton.FlatAppearance.BorderSize = 0
        AddHandler logoutButton.Click, AddressOf LogoutButton_Click
        AddHandler logoutButton.MouseEnter, AddressOf LogoutButton_MouseEnter
        AddHandler logoutButton.MouseLeave, AddressOf LogoutButton_MouseLeave
        pnlTopNav.Controls.Add(logoutButton)
    End Sub

    Private Sub CreateWhyChooseSection()
        ' Main container for why choose and image
        Dim sectionContainer As New Panel With {
        .Height = 280,
        .Location = New Point(0, 80),
        .Width = Me.Width,
        .BackColor = Color.Transparent,
        .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
    }
        ' Why Choose panel - white semi-transparent
        Dim whyChoosePanel As New Panel()
        whyChoosePanel.BackColor = Color.FromArgb(200, 255, 255, 255)
        whyChoosePanel.Location = New Point(50, 30)
        whyChoosePanel.Size = New Size(700, 220)
        sectionContainer.Controls.Add(whyChoosePanel)
        ' Title
        Dim titleLabel As New Label With {
        .Text = "WHY CHOOSE LAKBAY PH",
        .Font = New Font("Arial", 20, FontStyle.Bold),
        .ForeColor = Color.FromArgb(6, 41, 55),
        .Location = New Point(30, 20),
        .Size = New Size(440, 30),
        .BackColor = Color.Transparent
    }
        whyChoosePanel.Controls.Add(titleLabel)
        ' Feature boxes - 4 solid white panels
        Dim features As String(,) = {
        {"🌍", "500+ Destinations", "Explore amazing places worldwide"},
        {"💎", "Premium Experiences", "Luxury travel and unique adventures"},
        {"🎯", "Expert Guidance", "Professional travel consultants"},
        {"🏆", "Award Winning", "Recognized for excellence in travel"}
    }
        For i As Integer = 0 To 3
            Dim featureBox As New Panel With {
            .Size = New Size(145, 120),
            .Location = New Point(30 + (i * 165), 70),
            .BackColor = Color.White
        }
            Dim iconLabel As New Label With {
            .Text = features(i, 0),
            .Font = New Font("Arial", 24),
            .Location = New Point(30, 10),
            .Size = New Size(50, 40),
            .TextAlign = ContentAlignment.MiddleCenter,
            .BackColor = Color.Transparent
        }
            featureBox.Controls.Add(iconLabel)
            Dim featureLabel As New Label With {
            .Text = features(i, 1),
            .Font = New Font("Arial", 11, FontStyle.Bold),
            .ForeColor = Color.FromArgb(6, 41, 55),
            .Location = New Point(15, 55),
            .Size = New Size(120, 20),
            .TextAlign = ContentAlignment.MiddleCenter,
            .BackColor = Color.Transparent
        }
            featureBox.Controls.Add(featureLabel)
            Dim descLabel As New Label With {
            .Text = features(i, 2),
            .Font = New Font("Segoe UI", 9),
            .Location = New Point(15, 80),
            .Size = New Size(120, 30),
            .TextAlign = ContentAlignment.MiddleCenter,
            .ForeColor = Color.FromArgb(120, 120, 120),
            .BackColor = Color.Transparent
        }
            featureBox.Controls.Add(descLabel)
            whyChoosePanel.Controls.Add(featureBox)
        Next
        ' Image panel - same height and width as why choose panel
        Dim imagePanel As New Panel With {
        .Size = New Size(700, 220),
        .Location = New Point(sectionContainer.Width - 750, 30),
        .BackColor = Color.LightBlue,
        .Anchor = AnchorStyles.Top Or AnchorStyles.Right
    }

        ' Add whyImg PictureBox to the image panel
        Dim whyImg As New PictureBox With {
        .Size = New Size(700, 220),
        .Location = New Point(0, 0),
        .SizeMode = PictureBoxSizeMode.StretchImage,
        .BackColor = Color.Transparent,
        .Cursor = Cursors.Hand
    }

        ' Load image with fallback paths
        Try
            Dim imagePath As String = Path.Combine(Application.StartupPath, "fdimg.png")
            If Not File.Exists(imagePath) Then
                imagePath = Path.Combine(Application.StartupPath, "..\..\fdimg.png")
            End If
            If File.Exists(imagePath) Then
                whyImg.Image = Image.FromFile(imagePath)
                whyImg.SizeMode = PictureBoxSizeMode.StretchImage
            Else
                whyImg.BackColor = Color.LightBlue
            End If
        Catch
            whyImg.BackColor = Color.LightBlue
        End Try

        ' Add click event handler to redirect to freediving form
        AddHandler whyImg.Click, AddressOf WhyImg_Click

        imagePanel.Controls.Add(whyImg)
        sectionContainer.Controls.Add(imagePanel)
        mainPanel.Controls.Add(sectionContainer)
    End Sub

    Private Sub CreateDestinationsSection()
        ' Popular Destinations panel with maximized sizing
        destinationsPanel = New Panel With {
        .Height = 800,
        .Location = New Point(0, 370),
        .Width = Me.Width,
        .BackColor = Color.Transparent,
        .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
    }

        ' Gradient background panel - maximized width with minimal margins
        gradientPanel = New Panel()
        gradientPanel.Location = New Point(15, 15)
        gradientPanel.Size = New Size(destinationsPanel.Width - 50, 770)
        gradientPanel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        AddHandler gradientPanel.Paint, AddressOf GradientPanel_Paint
        destinationsPanel.Controls.Add(gradientPanel)

        ' Title section with better contrast
        Dim titleLabel As New Label With {
        .Text = "Popular Destinations",
        .Font = New Font("Arial", 36, FontStyle.Bold),
        .ForeColor = Color.White,
        .Location = New Point(40, 25),
        .Size = New Size(700, 50),
        .BackColor = Color.Transparent
    }
        gradientPanel.Controls.Add(titleLabel)

        Dim subtitleLabel As New Label With {
        .Text = "Discover amazing places around the world",
        .Font = New Font("Arial", 20, FontStyle.Italic),
        .ForeColor = Color.White,
        .Location = New Point(40, 80),
        .Size = New Size(800, 40),
        .BackColor = Color.Transparent
    }
        gradientPanel.Controls.Add(subtitleLabel)

        ' Destination data with image paths
        Dim destinations As String(,) = {
        {"Batangas Sunset Bay", "₱2,800", "1D 0N", "Freediving", "fdbatangas.png"},
        {"Boracay, Philippines", "₱22,000", "4D 3N", "Domestic", "domboracay.png"},
        {"Tokyo, Japan", "₱85,000", "7D 6N", "International", "inttokyo.png"},
        {"Bangkok, Thailand", "₱45,000", "6D 5N", "International", "intbangkok.png"},
        {"Mabini Diving Sites", "₱7,500", "3D 2N", "Freediving", "fdmabini.png"},
        {"Bohol, Philippines", "₱6,750", "4D 3N", "Domestic", "dombohol.png"}
    }

        ' Calculate center positioning for 3 columns with bigger cards
        Dim panelWidth As Integer = gradientPanel.Width
        Dim cardWidth As Integer = 360  ' Increased from 280
        Dim cardHeight As Integer = 280  ' Increased from 220
        Dim gapBetweenCards As Integer = 100  ' Reduced gap to maximize card size
        Dim totalCardsWidth As Integer = cardWidth * 3 + gapBetweenCards * 2
        Dim startX As Integer = (panelWidth - totalCardsWidth) / 2

        For i As Integer = 0 To 5
            Dim row As Integer = i \ 3
            Dim col As Integer = i Mod 3

            ' Main destination card with rounded corners - bigger size
            Dim destinationCard As New Panel With {
            .Size = New Size(cardWidth, cardHeight),
            .Location = New Point(startX + 80 + (col * (cardWidth + gapBetweenCards)), 140 + (row * (cardHeight + 35))),
            .BackColor = Color.White
        }
            AddHandler destinationCard.Paint, AddressOf DestinationCard_Paint_New

            ' Image panel (top portion) - bigger image area
            Dim imagePanel As New Panel With {
            .Size = New Size(cardWidth - 30, 150),  ' Increased from 130
            .Location = New Point(15, 15),
            .BackColor = Color.FromArgb(200, 180, 140)
        }
            AddHandler imagePanel.Paint, AddressOf ImagePanel_Paint
            destinationCard.Controls.Add(imagePanel)

            ' Set image names for each card
            Dim imageNames() As String = {"fdbatangas.png", "domboracay.png", "inttokyo.png", "intbangkok.png", "fdmabini.png", "dombohol.png"}
            imagePanel.Tag = imageNames(i)

            Dim desImg As New PictureBox With {
            .Size = New Size(cardWidth, 180),
            .Location = New Point(0, 0),
            .SizeMode = PictureBoxSizeMode.StretchImage,
            .BackColor = Color.FromArgb(200, 180, 140)
        }

            ' Load image with fallback paths
            Try
                Dim imagePath As String = Path.Combine(Application.StartupPath, imageNames(i))
                If Not File.Exists(imagePath) Then
                    imagePath = Path.Combine(Application.StartupPath, "..\.." & imageNames(i))
                End If
                If File.Exists(imagePath) Then
                    desImg.Image = Image.FromFile(imagePath)
                    desImg.SizeMode = PictureBoxSizeMode.StretchImage
                Else
                    desImg.BackColor = Color.FromArgb(200, 180, 140)
                End If
            Catch
                desImg.BackColor = Color.FromArgb(200, 180, 140)
            End Try

            imagePanel.Controls.Add(desImg)

            ' Content panel (bottom portion) - adjusted for bigger cards
            Dim contentPanel As New Panel With {
            .Size = New Size(cardWidth, cardHeight - 170),  ' Increased from 90
            .Location = New Point(0, 180),     ' Adjusted position
            .BackColor = Color.White
        }
            destinationCard.Controls.Add(contentPanel)

            ' Destination name - bigger font
            Dim nameLabel As New Label With {
            .Text = destinations(i, 0),
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),  ' Increased from 14
            .Location = New Point(20, 0),  ' Adjusted position
            .Size = New Size(320, 28),      ' Increased width
            .ForeColor = Color.FromArgb(60, 60, 60),
            .BackColor = Color.Transparent
        }
            contentPanel.Controls.Add(nameLabel)

            ' Price - bigger font
            Dim priceLabel As New Label With {
            .Text = destinations(i, 1),
            .Font = New Font("Segoe UI", 14, FontStyle.Bold),  ' Increased from 12
            .Location = New Point(20, 30),  ' Adjusted position
            .Size = New Size(120, 22),      ' Increased size
            .ForeColor = Color.FromArgb(80, 80, 80),
            .BackColor = Color.Transparent
        }
            contentPanel.Controls.Add(priceLabel)

            ' Duration - bigger font
            Dim durationLabel As New Label With {
            .Text = destinations(i, 2),
            .Font = New Font("Segoe UI", 14, FontStyle.Bold),  ' Increased from 10
            .Location = New Point(20, 57),    ' Adjusted position
            .Size = New Size(80, 22),          ' Increased size
            .ForeColor = Color.FromArgb(120, 120, 120),
            .BackColor = Color.Transparent
        }
            contentPanel.Controls.Add(durationLabel)

            ' Read More button - bigger and better positioned
            Dim readMoreBtn As New Button With {
            .Text = "Read More ⊳",
            .Size = New Size(110, 30),      ' Increased from 90x25
            .Location = New Point(230, 50), ' Adjusted position
            .BackColor = Color.FromArgb(70, 130, 140),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),  ' Increased font
            .Cursor = Cursors.Hand,
            .Tag = destinations(i, 3)
        }
            readMoreBtn.FlatAppearance.BorderSize = 0
            AddHandler readMoreBtn.Click, AddressOf ReadMoreButton_Click
            contentPanel.Controls.Add(readMoreBtn)

            gradientPanel.Controls.Add(destinationCard)
        Next

        mainPanel.Controls.Add(destinationsPanel)
    End Sub

    Private Sub DestinationCard_Paint_New(sender As Object, e As PaintEventArgs)
        Dim panel As Panel = CType(sender, Panel)
        Dim rect As New Rectangle(0, 0, panel.Width, panel.Height)

        ' Create rounded rectangle path
        Dim path As New Drawing2D.GraphicsPath()
        Dim radius As Integer = 15
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90)
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90)
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90)
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90)
        path.CloseFigure()

        ' Set the region to create rounded corners
        panel.Region = New Region(path)

        ' Draw shadow effect
        Using shadowBrush As New SolidBrush(Color.White)
            e.Graphics.FillPath(shadowBrush, path)
        End Using
    End Sub

    Private Sub ImagePanel_Paint(sender As Object, e As PaintEventArgs)
        Dim panel As Panel = CType(sender, Panel)
        Dim imageName As String = If(panel.Tag IsNot Nothing, panel.Tag.ToString(), "")

        If String.IsNullOrEmpty(imageName) Then Return

        Try
            Dim imagePath As String = Path.Combine(Application.StartupPath, "images", imageName)

            If File.Exists(imagePath) Then
                Using img As Image = Image.FromFile(imagePath)
                    ' Draw rounded corners for top only
                    Dim rect As New Rectangle(0, 0, panel.Width, panel.Height)
                    Dim path As New Drawing2D.GraphicsPath()
                    Dim radius As Integer = 15
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90)
                    path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90)
                    path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom)
                    path.CloseFigure()

                    ' Set clipping region for rounded corners
                    e.Graphics.SetClip(path)

                    ' Draw the image to fill the entire panel
                    e.Graphics.DrawImage(img, rect)

                    ' Reset clipping
                    e.Graphics.ResetClip()

                    ' Set the panel region for rounded corners
                    panel.Region = New Region(path)
                End Using
            Else
                ' Fallback: Draw gradient background if image not found
                Using brush As New Drawing2D.LinearGradientBrush(panel.ClientRectangle,
                                                             Color.FromArgb(220, 200, 160),
                                                             Color.FromArgb(180, 140, 100),
                                                             Drawing2D.LinearGradientMode.Vertical)
                    e.Graphics.FillRectangle(brush, panel.ClientRectangle)
                End Using

                ' Draw "Image Not Found" text
                Using font As New Font("Segoe UI", 10, FontStyle.Italic)
                    Using textBrush As New SolidBrush(Color.White)
                        Dim text As String = "Image Not Found"
                        Dim textSize As SizeF = e.Graphics.MeasureString(text, font)
                        Dim textX As Single = (panel.Width - textSize.Width) / 2
                        Dim textY As Single = (panel.Height - textSize.Height) / 2
                        e.Graphics.DrawString(text, font, textBrush, textX, textY)
                    End Using
                End Using

                ' Still apply rounded corners
                Dim rect As New Rectangle(0, 0, panel.Width, panel.Height)
                Dim path As New Drawing2D.GraphicsPath()
                Dim radius As Integer = 15
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90)
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90)
                path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom)
                path.CloseFigure()
                panel.Region = New Region(path)
            End If

        Catch ex As Exception
            ' Error loading image - show fallback
            Using brush As New Drawing2D.LinearGradientBrush(panel.ClientRectangle,
                                                         Color.FromArgb(220, 200, 160),
                                                         Color.FromArgb(180, 140, 100),
                                                         Drawing2D.LinearGradientMode.Vertical)
                e.Graphics.FillRectangle(brush, panel.ClientRectangle)
            End Using

            Console.WriteLine($"Error loading image {imageName}: {ex.Message}")
        End Try
    End Sub

    Private Sub WhyImg_Click(sender As Object, e As EventArgs)
        Try
            ' Hide current form
            Me.Close()

            ' Create and show the freediving form
            Dim freedivingForm As New FreedivingPackagesForms()
            freedivingForm.Show()

        Catch ex As Exception
            MessageBox.Show("Error opening Freediving form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ReadMoreButton_Click(sender As Object, e As EventArgs)
        Try
            Dim clickedButton As Button = CType(sender, Button)
            Dim category As String = CStr(clickedButton.Tag)

            Select Case category.ToUpper()
                Case "DOMESTIC"
                    Dim dompackages As New DomesticTravelForms()
                    dompackages.Show()
                    Me.Close()

                Case "INTERNATIONAL"
                    Dim intnl As New InternationalTravelForms()
                    intnl.Show()
                    Me.Close()

                Case "FREEDIVING"
                    Dim diving As New FreedivingPackagesForms()
                    diving.Show()
                    Me.Close()

                Case Else
                    ' Default fallback - could redirect to general packages form
                    MessageBox.Show("Package category not recognized. Redirecting to general packages.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Dim packagesForm As New LakbayPHPackagesForm()
                    packagesForm.Show()
                    Me.Close()
            End Select

        Catch ex As Exception
            MessageBox.Show("Unable to open the selected package form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub CreateFooter()
        footerPanel = New Panel With {
            .Height = 80,
            .Location = New Point(0, 1080),
            .Width = Me.Width,
            .BackColor = Color.FromArgb(6, 41, 55),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        }

        ' Company name and copyright
        Dim footerLabel As New Label With {
            .Text = "2025 Lakbay PH Travel And Tours rights reserved",
            .Font = New Font("Arial", 11, FontStyle.Bold),
            .ForeColor = Color.White,
            .AutoSize = True,
            .Location = New Point(60, 20)
        }
        footerPanel.Controls.Add(footerLabel)

        ' Contact information
        Dim contactLabel As New Label With {
            .Text = "Contact Us: +63 917 123 4567 | info@lakbayphtours.com | www.lakbayphtours.com",
            .Font = New Font("Arial", 10),
            .ForeColor = Color.White,
            .AutoSize = True,
            .Location = New Point(60, 45)
        }
        footerPanel.Controls.Add(contactLabel)

        ' Social media links (right aligned)
        Dim socialLabel As New Label With {
            .Text = "Follow Us: Facebook | Instagram | Twitter",
            .Font = New Font("Arial", 10),
            .ForeColor = Color.White,
            .AutoSize = True,
            .Location = New Point(footerPanel.Width - 300, 32),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right
        }
        footerPanel.Controls.Add(socialLabel)

        mainPanel.Controls.Add(footerPanel)
    End Sub

    ' Paint event handlers
    Private Sub GradientPanel_Paint(sender As Object, e As PaintEventArgs)
        Dim rect As New Rectangle(0, 0, sender.Width, sender.Height)

        ' Create a solid background color instead of gradient to remove white gradient
        Using brush As New SolidBrush(Color.FromArgb(160, 6, 41, 55))
            e.Graphics.FillRectangle(brush, rect)
        End Using

        ' Add a subtle overlay for better text readability
        Using overlayBrush As New SolidBrush(Color.FromArgb(20, 0, 0, 0))
            e.Graphics.FillRectangle(overlayBrush, rect)
        End Using
    End Sub

    Private Sub DestinationCard_Paint(sender As Object, e As PaintEventArgs)
        Dim rect As New Rectangle(0, 0, sender.Width - 1, sender.Height - 1)
        Using path As GraphicsPath = CreateRoundedRectangle(rect, 15)
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
            e.Graphics.FillPath(New SolidBrush(Color.FromArgb(250, 255, 255, 255)), path)
            e.Graphics.DrawPath(New Pen(Color.FromArgb(150, 150, 150), 2), path)
        End Using
    End Sub

    ' Button event handlers
    Private Sub PackagesButton_Click(sender As Object, e As EventArgs)
        Try
            Dim packagesForm As New LakbayPHPackagesForm()
            packagesForm.Show()
            Me.Close()
        Catch ex As Exception
            MessageBox.Show("Unable to open Packages form.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub AboutUsButton_Click(sender As Object, e As EventArgs)
        Dim about As New AboutUsForm()
        about.Show()
    End Sub

    Private Sub ProfileButton_Click(sender As Object, e As EventArgs)
        Dim userId As Integer = CType(Application.OpenForms("MainForm"), MainForm).CurrentUser.UserID
        Dim userprofile As New UserProfileForm(userId)
        userprofile.Show()
    End Sub


    Private Sub ListButton_Click(sender As Object, e As EventArgs)
        Dim bookinglist As New BookingListForm()
        bookinglist.Show()
    End Sub
    ' Modified Book Now button click handler with category-based routing
    Private Sub BookNowButton_Click(sender As Object, e As EventArgs)
        Try
            Dim clickedButton As Button = CType(sender, Button)
            Dim category As String = CStr(clickedButton.Tag)

            Select Case category.ToUpper()
                Case "DOMESTIC"
                    Dim dompackages As New DomesticTravelForms()
                    dompackages.Show()
                    Me.Close()

                Case "INTERNATIONAL"
                    Dim intnl As New InternationalTravelForms()
                    intnl.Show()
                    Me.Close()

                Case "FREEDIVING"
                    Dim diving As New FreedivingPackagesForms()
                    diving.Show()
                    Me.Close()

                Case Else
                    ' Default fallback - could redirect to general packages form
                    MessageBox.Show("Package category not recognized. Redirecting to general packages.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Dim packagesForm As New LakbayPHPackagesForm()
                    packagesForm.Show()
                    Me.Close()
            End Select

        Catch ex As Exception
            MessageBox.Show("Unable to open the selected package form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub LogoutButton_Click(sender As Object, e As EventArgs)
        Dim result As DialogResult = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If result = DialogResult.Yes Then
            Try
                user = Nothing
                Dim mainForm As New MainForm()
                mainForm.Show()
                Me.Close()
            Catch ex As Exception
                MessageBox.Show("Error during logout: " & ex.Message, "Logout Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    Private Sub LogoutButton_MouseEnter(sender As Object, e As EventArgs)
        Dim btn As Button = CType(sender, Button)
        btn.BackColor = Color.FromArgb(200, 35, 51)
    End Sub

    Private Sub LogoutButton_MouseLeave(sender As Object, e As EventArgs)
        Dim btn As Button = CType(sender, Button)
        btn.BackColor = Color.FromArgb(6, 41, 55)
    End Sub

    ' Helper method for rounded rectangles
    Private Function CreateRoundedRectangle(rect As Rectangle, radius As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90)
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90)
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90)
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90)
        path.CloseAllFigures()
        Return path
    End Function

    Private Sub TravelHomepageForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.WindowState = FormWindowState.Maximized
    End Sub
End Class