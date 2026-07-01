Imports System.Drawing
Imports System.Windows.Forms
Imports System.IO

Public Class LakbayPHPackagesForm
    Inherits Form

    Private components As System.ComponentModel.IContainer

    ' Navigation controls
    Private pnlNavigation As Panel
    Private logoBox As PictureBox
    Private lblCompanyName As Label
    Private btnHome As Button
    Private btnPackages As Button
    Private btnAboutUs As Button
    Private btnUserProfile As Button
    Private listButton As Button

    ' Package cards
    Private pnlDomestic As Panel
    Private pnlInternational As Panel
    Private pnlFreediving As Panel
    Private lblDomestic As Label
    Private lblInternational As Label
    Private lblFreediving As Label

    ' Image containers for later use
    Private picDomestic As PictureBox
    Private picInternational As PictureBox
    Private picFreediving As PictureBox

    ' Background panel
    Private pnlBackground As Panel
    Private user As MainForm.UserInfo

    Public Sub New()
        InitializeComponent()
        SetupForm()
        SetupNavigation()
        SetupPackageCards()
    End Sub

    Public Sub New(user As MainForm.UserInfo)
        Me.user = user
        InitializeComponent()
        SetupForm()
        SetupNavigation()
        SetupPackageCards()
    End Sub

    Private Sub InitializeComponent()
        Me.pnlNavigation = New System.Windows.Forms.Panel()
        Me.logoBox = New System.Windows.Forms.PictureBox()
        Me.lblCompanyName = New System.Windows.Forms.Label()
        Me.btnHome = New System.Windows.Forms.Button()
        Me.btnPackages = New System.Windows.Forms.Button()
        Me.btnAboutUs = New System.Windows.Forms.Button()
        Me.listButton = New System.Windows.Forms.Button()
        Me.btnUserProfile = New System.Windows.Forms.Button()
        Me.pnlBackground = New System.Windows.Forms.Panel()
        Me.pnlDomestic = New System.Windows.Forms.Panel()
        Me.pnlInternational = New System.Windows.Forms.Panel()
        Me.pnlFreediving = New System.Windows.Forms.Panel()
        Me.lblDomestic = New System.Windows.Forms.Label()
        Me.lblInternational = New System.Windows.Forms.Label()
        Me.lblFreediving = New System.Windows.Forms.Label()
        Me.picDomestic = New System.Windows.Forms.PictureBox()
        Me.picInternational = New System.Windows.Forms.PictureBox()
        Me.picFreediving = New System.Windows.Forms.PictureBox()
        Me.SuspendLayout()
        Me.DoubleBuffered = True

        '
        'LakbayPHPackagesForm
        '
        Me.ClientSize = New System.Drawing.Size(1382, 753)
        Me.Controls.Add(Me.pnlNavigation)
        Me.Controls.Add(Me.pnlBackground)
        Me.Name = "LakbayPHPackagesForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "LakbayPH - Travel Packages"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized

        ' Set background image
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

        Me.ResumeLayout(False)
    End Sub

    Private Sub SetupForm()
        ' Form resize handler
        AddHandler Me.Resize, AddressOf Form_Resize
    End Sub

    Private Sub Form_Resize(sender As Object, e As EventArgs)
        ' Reposition elements on resize
        If Me.WindowState = FormWindowState.Maximized Then
            ResizeComponents()
        End If
    End Sub

    Private Sub ResizeComponents()
        ' Adjust component sizes based on form size
        If pnlBackground IsNot Nothing Then
            pnlBackground.Size = New Size(Me.ClientSize.Width, Me.ClientSize.Height - 80)
            RepositionPackageCards()
            RepositionNavigation()
        End If
    End Sub

    Private Sub SetupNavigation()
        ' Navigation Panel - Modern white header
        With Me.pnlNavigation
            .Location = New Point(0, 0)
            .Size = New Size(Me.Width, 70)
            .BackColor = Color.White
            .Dock = DockStyle.Top
            .BorderStyle = BorderStyle.None
        End With

        ' Logo PictureBox
        Me.logoBox = New PictureBox()
        With Me.logoBox
            .Size = New Size(40, 40)
            .Location = New Point(50, 15)
            Try
                Dim logoPath As String = Path.Combine(Application.StartupPath, "logo.png")
                If Not File.Exists(logoPath) Then
                    logoPath = Path.Combine(Application.StartupPath, "..\..\logo.png")
                End If
                If File.Exists(logoPath) Then
                    .Image = Image.FromFile(logoPath)
                    .SizeMode = PictureBoxSizeMode.StretchImage
                Else
                    .BackColor = Color.White
                End If
            Catch
                .BackColor = Color.White
            End Try
        End With

        ' Company Name Label
        With Me.lblCompanyName
            .Text = "LakbayPH" & vbCrLf & "Travel + Tours"
            .Font = New Font("Segoe UI", 12, FontStyle.Bold)
            .ForeColor = Color.FromArgb(6, 41, 55)
            .Location = New Point(100, 15)
            .Size = New Size(120, 40)
            .BackColor = Color.Transparent
            .TextAlign = ContentAlignment.MiddleLeft
        End With

        ' Calculate button positions for center alignment
        Dim buttonWidth As Integer = 100
        Dim buttonSpacing As Integer = 20
        Dim totalButtonWidth As Integer = 3 * buttonWidth + 2 * buttonSpacing
        Dim startX As Integer = (Me.Width - totalButtonWidth - 180) \ 2 + 100 ' Account for user profile button

        ' Navigation buttons
        btnHome = New Button()
        btnHome.Text = "Home"
        btnHome.Font = New Font("Arial", 11, FontStyle.Regular)
        btnHome.ForeColor = Color.FromArgb(6, 41, 55) ' #062937
        btnHome.FlatStyle = FlatStyle.Flat
        btnHome.FlatAppearance.BorderSize = 0
        btnHome.Location = New Point(400, 25)
        btnHome.Size = New Size(80, 30)
        AddHandler btnHome.Click, AddressOf BtnHome_Click  ' Add this line to connect the event handler

        btnPackages = New Button()
        btnPackages.Text = "Packages"
        btnPackages.Font = New Font("Arial", 11, FontStyle.Regular)
        btnPackages.ForeColor = Color.FromArgb(6, 41, 55) ' #062937
        btnPackages.FlatStyle = FlatStyle.Flat
        btnPackages.FlatAppearance.BorderSize = 0
        btnPackages.Location = New Point(500, 25)
        btnPackages.Size = New Size(100, 30)

        btnAboutUs = New Button()
        btnAboutUs.Text = "About Us"
        btnAboutUs.Font = New Font("Arial", 11, FontStyle.Regular)
        btnAboutUs.ForeColor = Color.FromArgb(6, 41, 55) ' #062937
        btnAboutUs.FlatStyle = FlatStyle.Flat
        btnAboutUs.FlatAppearance.BorderSize = 0
        btnAboutUs.Location = New Point(600, 25)
        btnAboutUs.Size = New Size(80, 30)

        ' Booking List Button
        With Me.listButton
            .Text = "📋"
            .Size = New Size(40, 40)
            .Location = New Point(Me.Width - 180, 15)
            .BackColor = Color.FromArgb(6, 41, 55)
            .ForeColor = Color.White
            .FlatStyle = FlatStyle.Flat
            .Font = New Font("Segoe UI", 12, FontStyle.Bold)
            .Cursor = Cursors.Hand
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right
            .FlatAppearance.BorderSize = 0
            AddHandler listButton.Click, AddressOf ListButton_Click
        End With

        ' User Profile Button - Modern circular design
        With Me.btnUserProfile
            .Text = "👤"
            .Font = New Font("Segoe UI", 12)
            .Location = New Point(Me.Width - 200, 15)
            .Size = New Size(40, 40)
            .BackColor = Color.FromArgb(6, 41, 55)
            .ForeColor = Color.White
            .FlatStyle = FlatStyle.Flat
            .FlatAppearance.BorderSize = 0
            .FlatAppearance.MouseOverBackColor = Color.FromArgb(6, 41, 55)
            .TextAlign = ContentAlignment.MiddleCenter
            .Cursor = Cursors.Hand
            AddHandler .Click, AddressOf BtnUserProfile_Click
        End With

        ' Add navigation controls to panel
        Me.pnlNavigation.Controls.Add(Me.logoBox)
        Me.pnlNavigation.Controls.Add(Me.lblCompanyName)
        Me.pnlNavigation.Controls.Add(Me.btnHome)
        Me.pnlNavigation.Controls.Add(Me.btnPackages)
        Me.pnlNavigation.Controls.Add(Me.btnAboutUs)
        Me.pnlNavigation.Controls.Add(Me.btnUserProfile)
        Me.pnlNavigation.Controls.Add(Me.listButton)
    End Sub

    Private Sub DrawActiveUnderline(sender As Object, e As PaintEventArgs)
        Dim btn As Button = DirectCast(sender, Button)
        Using pen As New Pen(Color.FromArgb(0, 150, 136), 3)
            e.Graphics.DrawLine(pen, 0, btn.Height - 3, btn.Width, btn.Height - 3)
        End Using
    End Sub

    Private Sub SetupPackageCards()
        ' Background Panel - will use form's background image
        With Me.pnlBackground
            .Location = New Point(0, 80)
            .Size = New Size(Me.Width, Me.Height - 80)
            .BackColor = Color.Transparent ' Make transparent to show form background
        End With

        ' Calculate card positions for modern layout
        Dim cardWidth As Integer = 300
        Dim cardHeight As Integer = 500
        Dim spacing As Integer = 60
        Dim totalWidth As Integer = 3 * cardWidth + 2 * spacing
        Dim startX As Integer = (Me.Width - totalWidth) \ 2
        Dim startY As Integer = 100

        ' Image names for each card
        Dim imageNames() As String = {"dom.png", "int.png", "fd.png"}

        ' Modern card setup with rounded corners effect
        SetupModernCard(Me.pnlDomestic, Me.lblDomestic, Me.picDomestic,
                   New Point(startX, startY), New Size(cardWidth, cardHeight),
                   "DOMESTIC", Color.FromArgb(255, 182, 193), Color.White,
                   AddressOf PnlDomestic_Click)

        ' Add image to Domestic card
        AddImageToCard(Me.pnlDomestic, imageNames(0), cardWidth, cardHeight)

        SetupModernCard(Me.pnlInternational, Me.lblInternational, Me.picInternational,
                   New Point(startX + cardWidth + spacing, startY), New Size(cardWidth, cardHeight),
                   "INTERNATIONAL", Color.FromArgb(230, 230, 250), Color.FromArgb(60, 60, 60),
                   AddressOf PnlInternational_Click)

        ' Add image to International card
        AddImageToCard(Me.pnlInternational, imageNames(1), cardWidth, cardHeight)

        SetupModernCard(Me.pnlFreediving, Me.lblFreediving, Me.picFreediving,
                   New Point(startX + 2 * (cardWidth + spacing), startY), New Size(cardWidth, cardHeight),
                   "FREEDIVING", Color.FromArgb(25, 25, 112), Color.White,
                   AddressOf PnlFreediving_Click)

        ' Add image to Freediving card
        AddImageToCard(Me.pnlFreediving, imageNames(2), cardWidth, cardHeight)

        ' Add cards to background panel
        Me.pnlBackground.Controls.Add(Me.pnlDomestic)
        Me.pnlBackground.Controls.Add(Me.pnlInternational)
        Me.pnlBackground.Controls.Add(Me.pnlFreediving)
    End Sub

    Private Sub AddImageToCard(cardPanel As Panel, imageName As String, cardWidth As Integer, cardHeight As Integer)
        ' Create PictureBox for the card image
        Dim cardImage As New PictureBox With {
        .Size = New Size(cardWidth, cardHeight), ' Leave space for text
        .Location = New Point(0, 0), ' Small margin from top and sides
        .SizeMode = PictureBoxSizeMode.StretchImage,
        .BackColor = Color.Transparent,
        .Cursor = Cursors.Hand
    }

        ' Load image with fallback paths
        Try
            Dim imagePath As String = Path.Combine(Application.StartupPath, imageName)
            If Not File.Exists(imagePath) Then
                imagePath = Path.Combine(Application.StartupPath, "..\..\", imageName)
            End If
            If File.Exists(imagePath) Then
                cardImage.Image = Image.FromFile(imagePath)
                cardImage.SizeMode = PictureBoxSizeMode.StretchImage
            Else
                ' Set a placeholder color if image not found
                cardImage.BackColor = Color.FromArgb(200, 200, 200)
            End If
        Catch
            cardImage.BackColor = Color.FromArgb(200, 200, 200)
        End Try

        ' Add click event to image (same as card click)
        Select Case imageName
            Case "dom.png"
                AddHandler cardImage.Click, AddressOf PnlDomestic_Click
            Case "int.png"
                AddHandler cardImage.Click, AddressOf PnlInternational_Click
            Case "fd.png"
                AddHandler cardImage.Click, AddressOf PnlFreediving_Click
        End Select

        ' Add image to the card panel
        cardPanel.Controls.Add(cardImage)

        ' Bring the image to front so it's visible
        cardImage.BringToFront()
    End Sub

    Private Sub SetupModernCard(panel As Panel, label As Label, pictureBox As PictureBox,
                               location As Point, size As Size, text As String,
                               backColor As Color, textColor As Color, clickHandler As EventHandler)
        ' Modern card panel with shadow effect
        With panel
            .Location = location
            .Size = size
            .BackColor = backColor
            .BorderStyle = BorderStyle.None
            .Cursor = Cursors.Hand
            ' Add rounded corners effect through region (optional)
            Dim path As New Drawing2D.GraphicsPath()
            Dim radius As Integer = 20
            path.AddArc(0, 0, radius, radius, 180, 90)
            path.AddArc(.Width - radius, 0, radius, radius, 270, 90)
            path.AddArc(.Width - radius, .Height - radius, radius, radius, 0, 90)
            path.AddArc(0, .Height - radius, radius, radius, 90, 90)
            path.CloseFigure()
            .Region = New Region(path)

            AddHandler .Click, clickHandler
            AddHandler .MouseEnter, AddressOf ModernCard_MouseEnter
            AddHandler .MouseLeave, AddressOf ModernCard_MouseLeave
        End With

        ' Picture box for images (you can add images later)
        With pictureBox
            .Location = New Point(0, 0)
            .Size = New Size(size.Width, size.Height - 80)
            .BackColor = Color.Transparent
            .SizeMode = PictureBoxSizeMode.StretchImage
            ' Placeholder for image loading
            ' .Image = Image.FromFile("your_image_path.jpg")
            .Cursor = Cursors.Hand
            AddHandler .Click, clickHandler
        End With

        ' Add controls to panel
        panel.Controls.Add(pictureBox)
        panel.Controls.Add(label)
    End Sub

    Private Sub RepositionPackageCards()
        Dim cardWidth As Integer = 300
        Dim cardHeight As Integer = 500
        Dim spacing As Integer = 60
        Dim totalWidth As Integer = 3 * cardWidth + 2 * spacing
        Dim startX As Integer = (Me.Width - totalWidth) \ 2
        Dim startY As Integer = 100

        pnlDomestic.Location = New Point(startX, startY)
        pnlInternational.Location = New Point(startX + cardWidth + spacing, startY)
        pnlFreediving.Location = New Point(startX + 2 * (cardWidth + spacing), startY)
    End Sub

    Private Sub RepositionNavigation()
        ' Reposition navigation buttons on resize
        Dim buttonWidth As Integer = 100
        Dim buttonSpacing As Integer = 20
        Dim totalButtonWidth As Integer = 3 * buttonWidth + 2 * buttonSpacing
        Dim startX As Integer = (Me.ClientSize.Width - totalButtonWidth - 180) \ 2 + 100

        btnHome.Location = New Point(startX, 25)
        btnPackages.Location = New Point(startX + buttonWidth + buttonSpacing, 25)
        btnAboutUs.Location = New Point(startX + 2 * (buttonWidth + buttonSpacing), 25)
        btnUserProfile.Location = New Point(Me.ClientSize.Width - 80, 15)
    End Sub

    ' Modern hover effects
    Private Sub ModernCard_MouseEnter(sender As Object, e As EventArgs)
        Dim panel As Panel = DirectCast(sender, Panel)
        ' Subtle scale effect
        panel.Location = New Point(panel.Location.X - 5, panel.Location.Y - 5)
        panel.Size = New Size(panel.Size.Width + 10, panel.Size.Height + 10)
    End Sub

    Private Sub ModernCard_MouseLeave(sender As Object, e As EventArgs)
        Dim panel As Panel = DirectCast(sender, Panel)
        ' Reset to original size
        panel.Location = New Point(panel.Location.X + 5, panel.Location.Y + 5)
        panel.Size = New Size(panel.Size.Width - 10, panel.Size.Height - 10)
    End Sub

    ' Navigation event handlers
    Private Sub BtnHome_Click(sender As Object, e As EventArgs)
        Try
            ' Check if we have user info to pass
            If user IsNot Nothing Then
                Dim homeForm As New TravelHomepageForm(user)
                homeForm.Show()
            Else
                Dim homeForm As New TravelHomepageForm()
                homeForm.Show()
            End If
            Me.Close()
        Catch ex As Exception
            MessageBox.Show("Error opening Home form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BtnAboutUs_Click(sender As Object, e As EventArgs)
        Dim aboutUs As New AboutUsForm()
        aboutUs.Show()
    End Sub

    Private Sub BtnUserProfile_Click(sender As Object, e As EventArgs)
        ' Replace 1 with the actual current user ID variable if available
        Try
            Dim userId As Integer = CType(Application.OpenForms("MainForm"), MainForm).CurrentUser.UserID
            Dim userprofile As New UserProfileForm(userId)
            userprofile.Show()
        Catch ex As Exception
            ' Handle case where MainForm is not available
            MessageBox.Show("User profile not available", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Try
    End Sub

    Private Sub ListButton_Click(sender As Object, e As EventArgs)
        Dim bookinglist As New BookingListForm()
        bookinglist.Show()
    End Sub

    ' Package card event handlers
    Private Sub PnlDomestic_Click(sender As Object, e As EventArgs)
        Dim dompackages As New DomesticTravelForm()
        dompackages.Show()
        Me.Hide()
    End Sub

    Private Sub PnlInternational_Click(sender As Object, e As EventArgs)
        Dim intnl As New InternationalTravelForm()
        intnl.Show()
        Me.Hide()
    End Sub

    Private Sub PnlFreediving_Click(sender As Object, e As EventArgs)
        Dim diving As New FreedivingPackagesForm()
        diving.Show()
        Me.Hide()
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing AndAlso (components IsNot Nothing) Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    Private Sub LakbayPHPackagesForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class

' Module to run the packages form
Module PackagesProgramssssse
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New LakbayPHPackagesForm())
    End Sub
End Module