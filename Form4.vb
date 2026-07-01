Imports System.Drawing
Imports System.Windows.Forms
Imports System.IO

Public Class AboutUsForm
    Inherits Form

    Private pnlTopNav As Panel
    Private logo As PictureBox
    Private lblLogo As Label
    Private pnlMainContent As Panel

    Public Sub New()
        InitializeComponent()
        SetupForm()
        CreateTopNavigation()
        CreateMainContent()
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        Me.Text = "LakbayPH - About Us"
        Me.Size = New Size(1200, 800)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.WindowState = FormWindowState.Maximized
        Me.FormBorderStyle = FormBorderStyle.Sizable
        Me.MinimumSize = New Size(1000, 600)
        Me.ResumeLayout(False)
        Me.DoubleBuffered = True

    End Sub

    Private Sub SetupForm()
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
        Me.FormBorderStyle = FormBorderStyle.Sizable
        Me.MinimumSize = New Size(1000, 600)
    End Sub

    Private Sub CreateTopNavigation()
        ' Top navigation panel with background
        pnlTopNav = New Panel()
        pnlTopNav.Size = New Size(Me.Width, 90)
        pnlTopNav.Location = New Point(0, 0)
        pnlTopNav.BackColor = Color.FromArgb(25, 77, 98)
        pnlTopNav.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Me.Controls.Add(pnlTopNav)

        ' Logo
        logo = New PictureBox()
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
        lblLogo = New Label()
        lblLogo.Text = "LakbayPH"
        lblLogo.Font = New Font("Arial", 25, FontStyle.Bold)
        lblLogo.ForeColor = Color.White
        lblLogo.Location = New Point(130, 15)
        lblLogo.Size = New Size(200, 40)
        lblLogo.TextAlign = ContentAlignment.MiddleLeft
        lblLogo.BackColor = Color.Transparent
        pnlTopNav.Controls.Add(lblLogo)

        ' Sub text
        Dim lblSubLogo = New Label()
        lblSubLogo.Text = "Discover the World with Us"
        lblSubLogo.Font = New Font("Arial", 12)
        lblSubLogo.ForeColor = Color.White
        lblSubLogo.Location = New Point(130, 57)
        lblSubLogo.Size = New Size(200, 20)
        lblSubLogo.TextAlign = ContentAlignment.MiddleLeft
        lblSubLogo.BackColor = Color.Transparent
        pnlTopNav.Controls.Add(lblSubLogo)
    End Sub

    Private Sub CreateMainContent()
        ' Enable auto-scroll for the form
        Me.AutoScroll = True
        Me.AutoScrollMinSize = New Size(1000, 1200)

        ' Create Mission Panel
        Dim pnlMission As New Panel()
        pnlMission.BackColor = Color.FromArgb(82, 255, 255, 255)
        pnlMission.Location = New Point(40, 120)
        pnlMission.Size = New Size(700, 220)

        Dim lblMissionTitle As New Label()
        lblMissionTitle.Text = "🎯 OUR MISSION"
        lblMissionTitle.Font = New Font("Arial", 26, FontStyle.Bold)
        lblMissionTitle.ForeColor = Color.FromArgb(6, 41, 55)
        lblMissionTitle.Location = New Point(30, 30)
        lblMissionTitle.Size = New Size(300, 40)
        lblMissionTitle.BackColor = Color.Transparent
        pnlMission.Controls.Add(lblMissionTitle)

        Dim lblMissionText As New Label()
        lblMissionText.Text = "Make the experience of traveling to your dream" & vbCrLf &
                         "tourist destination come true with us. We will provide" & vbCrLf &
                         "the bestexperience of your life with unforgettable" & vbCrLf &
                         "memories and authentic Filipino hospitality."
        lblMissionText.Font = New Font("Arial", 17)
        lblMissionText.ForeColor = Color.White
        lblMissionText.Location = New Point(90, 80)
        lblMissionText.Size = New Size(670, 150)
        lblMissionText.BackColor = Color.Transparent
        pnlMission.Controls.Add(lblMissionText)

        Me.Controls.Add(pnlMission)

        ' Create Vision Panel
        Dim pnlVision As New Panel()
        pnlVision.BackColor = Color.FromArgb(82, 255, 255, 255)
        pnlVision.Location = New Point(780, 120)
        pnlVision.Size = New Size(700, 220)

        Dim lblVisionTitle As New Label()
        lblVisionTitle.Text = "🌟 OUR VISION"
        lblVisionTitle.Font = New Font("Arial", 26, FontStyle.Bold)
        lblVisionTitle.ForeColor = Color.FromArgb(6, 41, 55)
        lblVisionTitle.Location = New Point(30, 30)
        lblVisionTitle.Size = New Size(300, 40)
        lblVisionTitle.BackColor = Color.Transparent
        pnlVision.Controls.Add(lblVisionTitle)

        Dim lblVisionText As New Label()
        lblVisionText.Text = "We envision a world where the Philippines is" & vbCrLf &
                            "recognized as the premier tropical destination in" & vbCrLf &
                            "Southeast Asia through sustainable tourism and" & vbCrLf &
                            "exceptional visitor experiences."
        lblVisionText.Font = New Font("Arial", 16)
        lblVisionText.ForeColor = Color.White
        lblVisionText.Location = New Point(90, 80)
        lblVisionText.Size = New Size(670, 150)
        lblVisionText.BackColor = Color.Transparent
        pnlVision.Controls.Add(lblVisionText)

        Me.Controls.Add(pnlVision)

        ' Create Values Panel
        Dim pnlValues As New Panel()
        pnlValues.BackColor = Color.FromArgb(82, 255, 255, 255)
        pnlValues.Location = New Point(40, 380)
        pnlValues.Size = New Size(1440, 220)

        Dim lblValuesTitle As New Label()
        lblValuesTitle.Text = "💎 OUR VALUES"
        lblValuesTitle.Font = New Font("Arial", 26, FontStyle.Bold)
        lblValuesTitle.ForeColor = Color.FromArgb(6, 41, 55)
        lblValuesTitle.Location = New Point(30, 30)
        lblValuesTitle.Size = New Size(300, 40)
        lblValuesTitle.BackColor = Color.Transparent
        pnlValues.Controls.Add(lblValuesTitle)

        Dim values() As String = {
        "🤝 Authenticity - We showcase genuine Filipino experiences",
        "🌱 Sustainability - We protect our environment and culture",
        "💖 Hospitality - We treat every guest like family",
        "🎨 Cultural Pride - We celebrate our rich heritage",
        "🔒 Safety - We prioritize your security and well-being",
        "✨ Excellence - We strive for the highest service standards"
    }

        For i As Integer = 0 To values.Length - 1
            Dim lblValuesText As New Label()
            lblValuesText.Text = values(i)
            lblValuesText.Font = New Font("Arial", 16, FontStyle.Bold)
            lblValuesText.ForeColor = Color.White
            lblValuesText.Location = New Point(110 + (i Mod 2) * 700, 80 + (i \ 2) * 35)
            lblValuesText.Size = New Size(520, 30)
            lblValuesText.BackColor = Color.Transparent
            pnlValues.Controls.Add(lblValuesText)
        Next

        Me.Controls.Add(pnlValues)

        ' Create Team Panel
        Dim pnlTeam As New Panel()
        pnlTeam.BackColor = Color.FromArgb(82, 255, 255, 255)
        pnlTeam.Location = New Point(40, 640)
        pnlTeam.Size = New Size(1440, 200)

        Dim lblTeamTitle As New Label()
        lblTeamTitle.Text = "👥 OUR TEAM"
        lblTeamTitle.Font = New Font("Arial", 26, FontStyle.Bold)
        lblTeamTitle.ForeColor = Color.FromArgb(6, 41, 55)
        lblTeamTitle.Location = New Point(30, 30)
        lblTeamTitle.Size = New Size(300, 40)
        lblTeamTitle.BackColor = Color.Transparent
        pnlTeam.Controls.Add(lblTeamTitle)

        Dim lblTeamText As New Label()
        lblTeamText.Text = "Our dedicated team of travel experts, local guides, and cultural ambassadors are passionate about sharing the wonders of the" & vbCrLf &
                          "Philippines. With years of experience in hospitality and tourism,we combine professional expertise with genuine Filipino warmth" & vbCrLf &
                          "to create unforgettable experiences that showcase the beauty, culture, and spirit of our beloved archipelago."
        lblTeamText.Font = New Font("Arial", 16)
        lblTeamText.ForeColor = Color.White
        lblTeamText.Location = New Point(90, 80)
        lblTeamText.Size = New Size(1410, 80)
        lblTeamText.BackColor = Color.Transparent
        pnlTeam.Controls.Add(lblTeamText)

        Me.Controls.Add(pnlTeam)

        ' Create Contact Panel (bottom section)
        Dim pnlBack As New Panel()
        pnlBack.BackColor = Color.FromArgb(82, 255, 255, 255)
        pnlBack.Location = New Point(40, 880)
        pnlBack.Size = New Size(1440, 290)

        Dim pnlContact As New Panel()
        pnlContact.BackColor = Color.FromArgb(25, 77, 98)
        pnlContact.Location = New Point(30, 30)
        pnlContact.Size = New Size(1390, 240)
        pnlBack.Controls.Add(pnlContact)

        Dim lblContactTitle As New Label()
        lblContactTitle.Text = "📞 Contact us"
        lblContactTitle.Font = New Font("Arial", 24, FontStyle.Bold)
        lblContactTitle.ForeColor = Color.White
        lblContactTitle.Location = New Point(30, 30)
        lblContactTitle.Size = New Size(300, 35)
        lblContactTitle.BackColor = Color.Transparent
        pnlContact.Controls.Add(lblContactTitle)

        Dim lblContactText As New Label()
        lblContactText.Text = "Ready to start your Philippine adventure? Contact us today!"
        lblContactText.Font = New Font("Arial", 14)
        lblContactText.ForeColor = Color.White
        lblContactText.Location = New Point(90, 80)
        lblContactText.Size = New Size(550, 20)
        lblContactText.BackColor = Color.Transparent
        pnlContact.Controls.Add(lblContactText)

        Dim lblContact As New Label()
        lblContact.Text = "📧 Email: info@lakbayph.com" & vbNewLine &
                         "📱 Phone: +63 2 8123 4567" & vbNewLine &
                         "🏢 Address: Manila, Philippines" & vbNewLine &
                         "🌐 Website: www.lakbayph.com"
        lblContact.Font = New Font("Arial", 14)
        lblContact.ForeColor = Color.White
        lblContact.Location = New Point(130, 110)
        lblContact.Size = New Size(550, 100)
        lblContact.BackColor = Color.Transparent
        pnlContact.Controls.Add(lblContact)

        logo = New PictureBox()
        logo.Size = New Size(75, 75)
        logo.Location = New Point(930, 35)
        pnlContact.Controls.Add(logo)

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

        lblLogo = New Label()
        lblLogo.Text = "LakbayPH"
        lblLogo.Font = New Font("Arial", 45, FontStyle.Bold)
        lblLogo.ForeColor = Color.White
        lblLogo.Location = New Point(1000, 35)
        lblLogo.Size = New Size(350, 80)
        lblLogo.TextAlign = ContentAlignment.MiddleLeft
        lblLogo.BackColor = Color.Transparent
        pnlContact.Controls.Add(lblLogo)

        ' Create Start Button
        Dim btnStart As New Button()
        btnStart.Text = "START YOUR JOURNEY"
        btnStart.Font = New Font("Arial", 14, FontStyle.Bold)
        btnStart.ForeColor = Color.White
        btnStart.BackColor = Color.FromArgb(28, 98, 125)
        btnStart.Location = New Point(990, 135)
        btnStart.Size = New Size(290, 50)
        btnStart.FlatStyle = FlatStyle.Flat
        btnStart.FlatAppearance.BorderColor = Color.White
        btnStart.FlatAppearance.BorderSize = 2
        AddHandler btnStart.Click, AddressOf BtnStart_Click
        pnlContact.Controls.Add(btnStart)

        Me.Controls.Add(pnlBack)
    End Sub

    Private Sub BtnStart_Click(sender As Object, e As EventArgs)
        ' Add your start button functionality here
        MessageBox.Show("Start your journey with LakbayPH!", "Get Started", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Me.Close()
    End Sub

    Private Sub AboutUsForm_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If pnlMainContent IsNot Nothing Then
            pnlMainContent.Size = New Size(Me.Width - 100, 600)
        End If
    End Sub
End Class

' Usage Example - Add this to your main form or startup
Public Class Programs
    Public Shared Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New AboutUsForm())
    End Sub
End Class