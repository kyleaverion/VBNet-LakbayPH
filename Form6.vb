Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

Public Class AboutUsFormwou
    Inherits Form

    Private headerPanel As Panel
    Private scrollPanel As Panel
    Private contentPanel As Panel
    Private logoLabel As Label
    Private titleLabel As Label
    Private missionPanel As Panel
    Private visionPanel As Panel
    Private valuesPanel As Panel
    Private teamPanel As Panel
    Private contactPanel As Panel
    Private fadeTimer As Timer
    Private currentAlpha As Integer = 0

    Public Sub New()
        InitializeComponent()
        SetupUI()
        ' Start fade animation after the form loads fully
        AddHandler Me.Load, AddressOf AboutUsForm_Load

        StartFadeInAnimation()
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()

        Me.FormBorderStyle = FormBorderStyle.None
        Me.WindowState = FormWindowState.Normal
        Me.Bounds = Screen.PrimaryScreen.Bounds
        Me.StartPosition = FormStartPosition.Manual
        Me.TopMost = True
        Me.BackColor = Color.FromArgb(240, 248, 255)
        Me.Font = New Font("Segoe UI", 10.0!)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ControlBox = False
        Me.Name = "AboutUsForm"
        Me.Text = "Lakbay Ph - About Us"
        Me.Opacity = 0 ' Start transparent
        Me.DoubleBuffered = True


        Me.ResumeLayout(False)
    End Sub

    Private Sub SetupUI()
        ' Scroll Panel
        scrollPanel = New Panel()
        scrollPanel.Dock = DockStyle.Fill
        scrollPanel.AutoScroll = True
        scrollPanel.BackColor = Color.White
        Me.Controls.Add(scrollPanel)

        ' Header Panel
        headerPanel = New Panel()
        headerPanel.Size = New Size(1200, 120)
        headerPanel.Location = New Point(0, 0)
        headerPanel.BackColor = Color.FromArgb(0, 123, 191)
        scrollPanel.Controls.Add(headerPanel)

        ' Logo Label
        logoLabel = New Label()
        logoLabel.Text = "🏝️ LAKBAY PH"
        logoLabel.Font = New Font("Segoe UI", 28, FontStyle.Bold)
        logoLabel.ForeColor = Color.White
        logoLabel.Size = New Size(300, 50)
        logoLabel.Location = New Point(50, 20)
        headerPanel.Controls.Add(logoLabel)

        ' Title Label
        titleLabel = New Label()
        titleLabel.Text = "Discover the Philippines with Us"
        titleLabel.Font = New Font("Segoe UI", 14, FontStyle.Italic)
        titleLabel.ForeColor = Color.FromArgb(220, 240, 255)
        titleLabel.Size = New Size(400, 30)
        titleLabel.Location = New Point(50, 70)
        headerPanel.Controls.Add(titleLabel)

        ' Content Panel
        contentPanel = New Panel()
        contentPanel.Location = New Point(0, 120)
        contentPanel.Size = New Size(1200, 1500) ' Adjust height based on content
        contentPanel.BackColor = Color.FromArgb(248, 252, 255)
        scrollPanel.Controls.Add(contentPanel)

        CreateMissionSection()
        CreateVisionSection()
        CreateValuesSection()
        CreateTeamSection()
        CreateContactSection()
    End Sub

    Private Sub CreateMissionSection()
        missionPanel = CreateStyledPanel(50, 30, "🎯 Our Mission")
        missionPanel.Size = New Size(500, 180)

        Dim missionText As New Label()
        missionText.Text = "At Lakbay Ph, we are passionate about showcasing the breathtaking beauty and rich cultural heritage of the Philippines. Our mission is to provide unforgettable travel experiences that connect you with the heart and soul of our beloved archipelago. We believe that every journey should be more than just a trip – it should be a transformative adventure that creates lasting memories and deep connections with the Filipino spirit."
        missionText.Font = New Font("Segoe UI", 10)
        missionText.ForeColor = Color.FromArgb(60, 60, 60)
        missionText.Size = New Size(450, 140)
        missionText.Location = New Point(20, 50)
        missionText.AutoSize = False
        missionPanel.Controls.Add(missionText)

        contentPanel.Controls.Add(missionPanel)
    End Sub

    Private Sub CreateVisionSection()
        visionPanel = CreateStyledPanel(580, 30, "🌟 Our Vision")
        visionPanel.Size = New Size(500, 180)

        Dim visionText As New Label()
        visionText.Text = "We envision a world where the Philippines is recognized as the premier tropical destination in Southeast Asia. Through sustainable tourism practices and authentic cultural experiences, we aim to be the bridge that connects international travelers with local communities, fostering mutual understanding and economic growth while preserving our natural wonders for future generations."
        visionText.Font = New Font("Segoe UI", 10)
        visionText.ForeColor = Color.FromArgb(60, 60, 60)
        visionText.Size = New Size(450, 140)
        visionText.Location = New Point(20, 50)
        visionText.AutoSize = False
        visionPanel.Controls.Add(visionText)

        contentPanel.Controls.Add(visionPanel)
    End Sub

    Private Sub CreateValuesSection()
        valuesPanel = CreateStyledPanel(50, 230, "💎 Our Values")
        valuesPanel.Size = New Size(1080, 200)

        Dim values() As String = {
            "🤝 Authenticity - We showcase genuine Filipino experiences",
            "🌱 Sustainability - We protect our environment and culture",
            "💖 Hospitality - We treat every guest like family",
            "🎨 Cultural Pride - We celebrate our rich heritage",
            "🔒 Safety - We prioritize your security and well-being",
            "✨ Excellence - We strive for the highest service standards"
        }

        For i As Integer = 0 To values.Length - 1
            Dim valueLabel As New Label()
            valueLabel.Text = values(i)
            valueLabel.Font = New Font("Segoe UI", 11)
            valueLabel.ForeColor = Color.FromArgb(70, 70, 70)
            valueLabel.Size = New Size(350, 50)
            valueLabel.Location = New Point(20 + (i Mod 3) * 350, 50 + (i \ 3) * 60)
            valueLabel.AutoSize = False
            valuesPanel.Controls.Add(valueLabel)
        Next

        contentPanel.Controls.Add(valuesPanel)
    End Sub

    Private Sub CreateTeamSection()
        teamPanel = CreateStyledPanel(50, 450, "👥 Our Team")
        teamPanel.Size = New Size(1080, 220)

        Dim teamDescription As New Label()
        teamDescription.Text = "Our dedicated team of travel experts, local guides, and cultural ambassadors are passionate about sharing the wonders of the Philippines. With years of experience in hospitality and tourism, we combine professional expertise with genuine Filipino warmth to ensure every aspect of your journey exceeds expectations."
        teamDescription.Font = New Font("Segoe UI", 11)
        teamDescription.ForeColor = Color.FromArgb(60, 60, 60)
        teamDescription.Size = New Size(1040, 90)
        teamDescription.Location = New Point(20, 50)
        teamDescription.AutoSize = False
        teamPanel.Controls.Add(teamDescription)

        ' Team Stats
        Dim statsPanel As New Panel()
        statsPanel.Size = New Size(1040, 70)
        statsPanel.Location = New Point(20, 140)
        statsPanel.BackColor = Color.FromArgb(240, 248, 255)

        Dim stats() As String = {"50+ Expert Guides", "10,000+ Happy Travelers", "100+ Destinations", "5 Years Experience"}
        For i As Integer = 0 To stats.Length - 1
            Dim statLabel As New Label()
            statLabel.Text = stats(i)
            statLabel.Font = New Font("Segoe UI", 12, FontStyle.Bold)
            statLabel.ForeColor = Color.FromArgb(0, 123, 191)
            statLabel.Size = New Size(250, 40)
            statLabel.Location = New Point(i * 260, 15)
            statLabel.TextAlign = ContentAlignment.MiddleCenter
            statsPanel.Controls.Add(statLabel)
        Next

        teamPanel.Controls.Add(statsPanel)
        contentPanel.Controls.Add(teamPanel)
    End Sub

    Private Sub CreateContactSection()
        contactPanel = CreateStyledPanel(50, 690, "📞 Get In Touch")
        contactPanel.Size = New Size(1080, 170)
        contactPanel.BackColor = Color.FromArgb(0, 123, 191)

        Dim contactInfo As New Label()
        contactInfo.Text = "Ready to start your Philippine adventure? Contact us today!" & vbNewLine & vbNewLine &
                          "📧 Email: info@lakbayph.com" & vbNewLine &
                          "📱 Phone: +63 2 8123 4567" & vbNewLine &
                          "🏢 Address: Manila, Philippines" & vbNewLine &
                          "🌐 Website: www.lakbayph.com"
        contactInfo.Font = New Font("Segoe UI", 12)
        contactInfo.ForeColor = Color.White
        contactInfo.Size = New Size(600, 120)
        contactInfo.Location = New Point(20, 50)
        contactInfo.AutoSize = False
        contactPanel.Controls.Add(contactInfo)

        ' Call to Action Button
        Dim ctaButton As New Button()
        ctaButton.Text = "Start Your Journey"
        ctaButton.Font = New Font("Segoe UI", 14, FontStyle.Bold)
        ctaButton.Size = New Size(200, 50)
        ctaButton.Location = New Point(800, 80)
        ctaButton.BackColor = Color.FromArgb(255, 193, 7)
        ctaButton.ForeColor = Color.FromArgb(33, 37, 41)
        ctaButton.FlatStyle = FlatStyle.Flat
        ctaButton.FlatAppearance.BorderSize = 0
        ctaButton.Cursor = Cursors.Hand
        AddHandler ctaButton.Click, AddressOf CtaButton_Click
        contactPanel.Controls.Add(ctaButton)

        contentPanel.Controls.Add(contactPanel)
    End Sub

    Private Function CreateStyledPanel(x As Integer, y As Integer, title As String) As Panel
        Dim panel As New Panel()
        panel.Size = New Size(500, 180)
        panel.Location = New Point(x, y)
        panel.BackColor = Color.White
        panel.BorderStyle = BorderStyle.None

        ' Add shadow effect
        AddHandler panel.Paint, AddressOf Panel_Paint

        ' Title Label
        Dim titleLabel As New Label()
        titleLabel.Text = title
        titleLabel.Font = New Font("Segoe UI", 16, FontStyle.Bold)
        titleLabel.ForeColor = Color.FromArgb(0, 123, 191)
        titleLabel.Size = New Size(460, 35)
        titleLabel.Location = New Point(20, 15)
        panel.Controls.Add(titleLabel)

        Return panel
    End Function

    Private Sub Panel_Paint(sender As Object, e As PaintEventArgs)
        Dim panel As Panel = CType(sender, Panel)
        Dim rect As New Rectangle(3, 3, panel.Width - 6, panel.Height - 6)

        ' Draw shadow
        Using shadowBrush As New SolidBrush(Color.FromArgb(50, 0, 0, 0))
            e.Graphics.FillRectangle(shadowBrush, New Rectangle(5, 5, panel.Width - 6, panel.Height - 6))
        End Using

        ' Draw panel background
        Using panelBrush As New SolidBrush(Color.White)
            e.Graphics.FillRectangle(panelBrush, rect)
        End Using

        ' Draw border
        Using borderPen As New Pen(Color.FromArgb(200, 200, 200), 1)
            e.Graphics.DrawRectangle(borderPen, rect)
        End Using
    End Sub

    Private Sub StartFadeInAnimation()
        fadeTimer = New Timer()
        fadeTimer.Interval = 50
        AddHandler fadeTimer.Tick, AddressOf FadeTimer_Tick
        fadeTimer.Start()
    End Sub

    Private Sub FadeTimer_Tick(sender As Object, e As EventArgs)
        If currentAlpha < 255 Then
            currentAlpha += 15
            Me.Opacity = currentAlpha / 255.0
        Else
            fadeTimer.Stop()
            Me.Opacity = 1.0
        End If
    End Sub

    Private Sub CtaButton_Click(sender As Object, e As EventArgs)



        ' Optional: Close the About Us form completely
        Me.Close()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)

        ' Create gradient background
        Using gradientBrush As New LinearGradientBrush(
            Me.ClientRectangle,
            Color.FromArgb(240, 248, 255),
            Color.FromArgb(220, 240, 255),
            LinearGradientMode.Vertical)

            e.Graphics.FillRectangle(gradientBrush, Me.ClientRectangle)
        End Using
    End Sub

    Private Sub AboutUsForm_Load(sender As Object, e As EventArgs)
        Me.Opacity = 0  ' Ensure starts invisible
        StartFadeInAnimation()
    End Sub

End Class

Public Class Programss
    Public Shared Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New AboutUsFormwou())
    End Sub
End Class