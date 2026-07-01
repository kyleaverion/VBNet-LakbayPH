Imports System.Drawing
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Data
Imports System.Windows.Forms
Imports Microsoft.Win32
Imports MySqlConnector
Imports System.ComponentModel
Imports MySql.Data.MySqlClient


Public Class MainForm
    Inherits Form

    ' Database connection
    Dim connectionString As String = "Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;"
    Dim conn As MySql.Data.MySqlClient.MySqlConnection = New MySql.Data.MySqlClient.MySqlConnection("Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;")
    Public sql As String
    Public cmd As MySql.Data.MySqlClient.MySqlCommand

    ' Top Navigation Controls
    Private logo As PictureBox
    Private lblLogo As Label
    Private pnlTopNav As Panel
    Private btnPackages As Button
    Private btnAboutUs As Button
    Private btnAdventurerHome As Button

    ' Login Panel Controls
    Private pnlLogin As Panel
    Private lblLogin As Label
    Private txtUsername As TextBox
    Private txtPassword As TextBox
    Private chkRememberMe As CheckBox
    Private btnLogIn As Button
    Private lblOr As Label
    Private btnSignUpLogin As Button

    ' Main Content Panel
    Private pnlMainContent As Panel
    Private lblMainTitle As Label
    Private lblSubtitle As Label
    Private logoMain As PictureBox

    ' Login status tracking
    Private loggedInUser As UserInfo = Nothing

    ' Remember Me
    Private Const REGISTRY_KEY_PATH As String = "SOFTWARE\LakbayPH"
    Private Const USERNAME_KEY As String = "RememberedUsername"
    Private Const PASSWORD_KEY As String = "RememberedPassword"
    Private Const REMEMBER_KEY As String = "RememberMe"

    ' User Info class to store logged-in user data
    Public Class UserInfo
        Public Property UserID As Integer
        Public Property Username As String
        Public Property Email As String
        Public Property FirstName As String
        Public Property LastName As String
        Public Property Phone As String
        Public Property Address As String
        Public Property BirthDate As DateTime?
        Public Property Gender As String
        Public Property Role As String
        Public Property IsActive As Boolean
        Public Property CreatedAt As DateTime
        Public Property UpdatedAt As DateTime

        ' Computed property for full name
        Public ReadOnly Property FullName As String
            Get
                Return $"{FirstName} {LastName}".Trim()
            End Get
        End Property

    End Class

    Public Sub New()
        Try
            InitializeComponent()
            SetupForm()
            CreateControls()
            SetupEventHandlers()
            LoadRememberedCredentials()
        Catch ex As Exception
            MessageBox.Show("Initialization error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1200, 800)
        Me.Name = "MainForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "LakbayPH Travel & Tours"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
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

    Private Sub CreateControls()
        Try
            ' Create top navigation bar
            CreateTopNavigation()
            ' Create main content area
            CreateMainContent()
            ' Create login panel
            CreateLoginPanel()
        Catch ex As Exception
            MessageBox.Show("Control creation error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub CreateTopNavigation()
        ' Top navigation panel with background
        pnlTopNav = New Panel()
        pnlTopNav.Size = New Size(Me.Width, 70)
        pnlTopNav.Location = New Point(0, 0)
        pnlTopNav.BackColor = Color.White
        pnlTopNav.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Me.Controls.Add(pnlTopNav)

        ' Logo
        logo = New PictureBox()
        logo.Size = New Size(40, 40)
        logo.Location = New Point(50, 15)
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
                logo.BackColor = Color.White
            End If
        Catch
            logo.BackColor = Color.White
        End Try
        pnlTopNav.Controls.Add(logo)

        ' Logo text
        lblLogo = New Label()
        lblLogo.Text = "LakbayPH" & vbCrLf & "Travel + Tours"
        lblLogo.Font = New Font("Arial", 11, FontStyle.Bold)
        lblLogo.ForeColor = Color.FromArgb(6, 41, 55) ' #062937
        lblLogo.Location = New Point(100, 15)
        lblLogo.Size = New Size(120, 40)
        pnlTopNav.Controls.Add(lblLogo)

        ' Navigation buttons
        btnPackages = New Button()
        btnPackages.Text = "Packages"
        btnPackages.Font = New Font("Arial", 11, FontStyle.Regular)
        btnPackages.ForeColor = Color.FromArgb(6, 41, 55) ' #062937
        btnPackages.FlatStyle = FlatStyle.Flat
        btnPackages.FlatAppearance.BorderSize = 0
        btnPackages.Location = New Point(380, 25)
        btnPackages.Size = New Size(100, 30)
        pnlTopNav.Controls.Add(btnPackages)

        btnAboutUs = New Button()
        btnAboutUs.Text = "About Us"
        btnAboutUs.Font = New Font("Arial", 11, FontStyle.Regular)
        btnAboutUs.ForeColor = Color.FromArgb(6, 41, 55) ' #062937
        btnAboutUs.FlatStyle = FlatStyle.Flat
        btnAboutUs.FlatAppearance.BorderSize = 0
        btnAboutUs.Location = New Point(500, 25)
        btnAboutUs.Size = New Size(80, 30)
        pnlTopNav.Controls.Add(btnAboutUs)

        btnAdventurerHome = New Button()
        btnAdventurerHome.Text = "Reviews"
        btnAdventurerHome.Font = New Font("Arial", 11, FontStyle.Regular)
        btnAdventurerHome.ForeColor = Color.FromArgb(6, 41, 55) ' #062937
        btnAdventurerHome.FlatStyle = FlatStyle.Flat
        btnAdventurerHome.FlatAppearance.BorderSize = 0
        btnAdventurerHome.Location = New Point(600, 25)
        btnAdventurerHome.Size = New Size(80, 30)
        pnlTopNav.Controls.Add(btnAdventurerHome)
    End Sub

    Private Sub CreateMainContent()
        ' Main content panel - bigger than login panel
        pnlMainContent = New Panel()
        pnlMainContent.BackColor = Color.FromArgb(82, 255, 255, 255) ' White with 32% transparency
        pnlMainContent.Location = New Point(50, 150) ' Higher position
        pnlMainContent.Size = New Size(Me.Width - 100, 500) ' Bigger than login panel
        pnlMainContent.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Me.Controls.Add(pnlMainContent)

        ' Calculate center position for content (between left edge of main content and left edge of login panel)
        Dim loginPanelStartX As Integer = Me.Width - 420 ' Login panel X position
        Dim mainContentLeftEdge As Integer = 50 ' Left edge of main content panel
        Dim availableWidth As Integer = loginPanelStartX - mainContentLeftEdge ' Space between main content left and login panel left
        Dim contentStartX As Integer = mainContentLeftEdge + (availableWidth - 650) \ 2 ' Center the content between these edges

        ' Company title
        Dim lblCompanyTitle As New Label()
        lblCompanyTitle.Text = "LakbayPH Travel And Tours"
        lblCompanyTitle.Font = New Font("Arial", 26, FontStyle.Bold)
        lblCompanyTitle.TextAlign = ContentAlignment.MiddleCenter
        lblCompanyTitle.ForeColor = Color.FromArgb(6, 41, 55)
        lblCompanyTitle.Location = New Point(contentStartX, 20)
        lblCompanyTitle.Size = New Size(600, 40)
        lblCompanyTitle.BackColor = Color.Transparent
        pnlMainContent.Controls.Add(lblCompanyTitle)

        ' Main title
        lblMainTitle = New Label()
        lblMainTitle.Text = "EXPLORE YOUR" & vbCrLf & "DREAM PLACE" & vbCrLf & "WITH US"
        lblMainTitle.TextAlign = ContentAlignment.MiddleCenter
        lblMainTitle.Font = New Font("Arial", 42, FontStyle.Bold)
        lblMainTitle.ForeColor = Color.FromArgb(6, 41, 55)
        lblMainTitle.Location = New Point(contentStartX, 75)
        lblMainTitle.Size = New Size(600, 200)
        lblMainTitle.BackColor = Color.Transparent
        pnlMainContent.Controls.Add(lblMainTitle)

        ' Subtitle
        lblSubtitle = New Label()
        lblSubtitle.Text = "Make the experience of traveling to your dream" & vbCrLf &
                         "tourist destination come true with us. We will provide" & vbCrLf &
                         "the best experience of your life."
        lblSubtitle.Font = New Font("Arial", 16, FontStyle.Italic)
        lblSubtitle.TextAlign = ContentAlignment.MiddleCenter
        lblSubtitle.ForeColor = Color.FromArgb(6, 41, 55)
        lblSubtitle.Location = New Point(contentStartX, 295)
        lblSubtitle.Size = New Size(600, 100)
        lblSubtitle.BackColor = Color.Transparent
        pnlMainContent.Controls.Add(lblSubtitle)

        ' Logo in main content
        logoMain = New PictureBox()
        logoMain.Size = New Size(60, 60)
        logoMain.Location = New Point(contentStartX + 250, 415) ' Centered within the content area
        logoMain.BackColor = Color.Transparent
        Try
            Dim logoPath As String = Path.Combine(Application.StartupPath, "logo.png")
            If Not File.Exists(logoPath) Then
                logoPath = Path.Combine(Application.StartupPath, "..\..\logo.png")
            End If
            If File.Exists(logoPath) Then
                logoMain.Image = Image.FromFile(logoPath)
                logoMain.SizeMode = PictureBoxSizeMode.StretchImage
            End If
        Catch
            logoMain.BackColor = Color.Transparent
        End Try
        pnlMainContent.Controls.Add(logoMain)
    End Sub

    Private Sub CreateLoginPanel()
        ' Login panel - positioned inside main content panel
        pnlLogin = New Panel()
        pnlLogin.BackColor = Color.FromArgb(25, 77, 98) ' #194d62
        pnlLogin.Location = New Point(Me.Width - 400, 200) ' Inside main content area
        pnlLogin.Size = New Size(350, 450)
        pnlLogin.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        Me.Controls.Add(pnlLogin)
        pnlLogin.BringToFront() ' Ensure it's on top

        ' Login title with airplane icon
        lblLogin = New Label()
        lblLogin.Text = "Log in ✈"
        lblLogin.Font = New Font("Arial", 28, FontStyle.Bold)
        lblLogin.ForeColor = Color.White
        lblLogin.Location = New Point(30, 30)
        lblLogin.Size = New Size(290, 50)
        lblLogin.BackColor = Color.Transparent
        pnlLogin.Controls.Add(lblLogin)

        ' Username textbox with icon
        Dim userIcon As New Label()
        userIcon.Text = "👤"
        userIcon.Font = New Font("Arial", 14)
        userIcon.ForeColor = Color.White
        userIcon.Location = New Point(30, 100)
        userIcon.Size = New Size(45, 30)
        userIcon.BackColor = Color.FromArgb(51, 92, 103)
        userIcon.TextAlign = ContentAlignment.MiddleCenter
        pnlLogin.Controls.Add(userIcon)

        txtUsername = New TextBox()
        txtUsername.Font = New Font("Arial", 14)
        txtUsername.Location = New Point(75, 100)
        txtUsername.Size = New Size(245, 30)
        txtUsername.BackColor = Color.FromArgb(51, 92, 103)
        txtUsername.ForeColor = Color.LightGray
        txtUsername.BorderStyle = BorderStyle.None
        txtUsername.Text = "Username or Email"
        txtUsername.Multiline = True
        txtUsername.TextAlign = HorizontalAlignment.Left
        Dim usernamePadding As New Padding(10, 12, 10, 12)
        txtUsername.Padding = usernamePadding
        pnlLogin.Controls.Add(txtUsername)

        ' Password textbox with icon
        Dim passIcon As New Label()
        passIcon.Text = "🔒"
        passIcon.Font = New Font("Arial", 14)
        passIcon.ForeColor = Color.White
        passIcon.Location = New Point(30, 160)
        passIcon.Size = New Size(45, 30)
        passIcon.BackColor = Color.FromArgb(51, 92, 103)
        passIcon.TextAlign = ContentAlignment.MiddleCenter
        pnlLogin.Controls.Add(passIcon)

        txtPassword = New TextBox()
        txtPassword.Font = New Font("Arial", 14)
        txtPassword.Location = New Point(75, 160)
        txtPassword.Size = New Size(245, 30)
        txtPassword.BackColor = Color.FromArgb(51, 92, 103)
        txtPassword.PasswordChar = "*"
        txtPassword.ForeColor = Color.LightGray
        txtPassword.BorderStyle = BorderStyle.None
        txtPassword.Text = "Password"
        txtPassword.Multiline = True
        txtPassword.TextAlign = HorizontalAlignment.Left
        txtPassword.UseSystemPasswordChar = False ' Initially false for placeholder
        Dim passwordPadding As New Padding(10, 12, 10, 12)
        txtPassword.Padding = passwordPadding
        pnlLogin.Controls.Add(txtPassword)

        ' Remember me checkbox
        chkRememberMe = New CheckBox()
        chkRememberMe.Text = "REMEMBER ME"
        chkRememberMe.Font = New Font("Arial", 10, FontStyle.Bold)
        chkRememberMe.ForeColor = Color.White
        chkRememberMe.Location = New Point(30, 220)
        chkRememberMe.Size = New Size(140, 20)
        chkRememberMe.BackColor = Color.Transparent
        pnlLogin.Controls.Add(chkRememberMe)

        ' Log in button
        btnLogIn = New Button()
        btnLogIn.Text = "LOG IN"
        btnLogIn.Font = New Font("Arial", 14, FontStyle.Bold)
        btnLogIn.ForeColor = Color.FromArgb(25, 77, 98)
        btnLogIn.BackColor = Color.White
        btnLogIn.FlatStyle = FlatStyle.Flat
        btnLogIn.FlatAppearance.BorderSize = 0
        btnLogIn.Location = New Point(30, 260)
        btnLogIn.Size = New Size(290, 50)
        pnlLogin.Controls.Add(btnLogIn)

        ' OR label
        lblOr = New Label()
        lblOr.Text = "OR"
        lblOr.Font = New Font("Arial", 14, FontStyle.Bold)
        lblOr.ForeColor = Color.White
        lblOr.Location = New Point(150, 325)
        lblOr.Size = New Size(50, 30)
        lblOr.TextAlign = ContentAlignment.MiddleCenter
        lblOr.BackColor = Color.Transparent
        pnlLogin.Controls.Add(lblOr)

        ' Sign up button in login panel
        btnSignUpLogin = New Button()
        btnSignUpLogin.Text = "SIGN UP"
        btnSignUpLogin.Font = New Font("Arial", 14, FontStyle.Bold)
        btnSignUpLogin.ForeColor = Color.White
        btnSignUpLogin.BackColor = Color.FromArgb(28, 98, 125) ' Solid color
        btnSignUpLogin.FlatStyle = FlatStyle.Flat
        btnSignUpLogin.FlatAppearance.BorderColor = Color.White
        btnSignUpLogin.FlatAppearance.BorderSize = 2
        btnSignUpLogin.Location = New Point(30, 370)
        btnSignUpLogin.Size = New Size(290, 50)
        pnlLogin.Controls.Add(btnSignUpLogin)
    End Sub

    Private Sub LoadRememberedCredentials()
        Try
            ' Ensure txtPassword is masked
            txtPassword.PasswordChar = "*"c

            Using key As RegistryKey = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY_PATH)
                If key IsNot Nothing Then
                    Dim rememberMe As String = key.GetValue(REMEMBER_KEY, "False").ToString()

                    If rememberMe = "True" Then
                        Dim savedUsername As String = key.GetValue(USERNAME_KEY, "").ToString()
                        Dim savedPassword As String = key.GetValue(PASSWORD_KEY, "").ToString()

                        If Not String.IsNullOrEmpty(savedUsername) AndAlso Not String.IsNullOrEmpty(savedPassword) Then
                            txtUsername.Text = savedUsername
                            txtUsername.ForeColor = Color.White
                            txtPassword.Text = DecryptPassword(savedPassword)
                            txtPassword.ForeColor = Color.White
                            chkRememberMe.Checked = True
                        End If
                    End If
                End If
            End Using
        Catch ex As Exception
            ' Silently handle registry errors
        End Try
    End Sub


    Private Sub SetupEventHandlers()
        AddHandler btnPackages.Click, AddressOf BtnPackages_Click
        AddHandler btnAboutUs.Click, AddressOf BtnAboutUs_Click
        AddHandler btnSignUpLogin.Click, AddressOf BtnSignUp_Click
        AddHandler btnLogIn.Click, AddressOf BtnLogIn_Click
        AddHandler btnAdventurerHome.Click, AddressOf BtnAdventurerHome_Click

        ' Add placeholder handlers for username and password textboxes
        AddHandler txtUsername.GotFocus, AddressOf TxtUsername_GotFocus
        AddHandler txtUsername.LostFocus, AddressOf TxtUsername_LostFocus
        AddHandler txtPassword.GotFocus, AddressOf TxtPassword_GotFocus
        AddHandler txtPassword.LostFocus, AddressOf TxtPassword_LostFocus

        ' Add Enter key handler for login
        AddHandler txtUsername.KeyPress, AddressOf TextBox_KeyPress
        AddHandler txtPassword.KeyPress, AddressOf TextBox_KeyPress
    End Sub


    ' Placeholder handlers for username textbox
    Private Sub TxtUsername_GotFocus(sender As Object, e As EventArgs)
        If txtUsername.Text = "Username or Email" Then
            txtUsername.Text = ""
            txtUsername.ForeColor = Color.White
        End If
    End Sub

    Private Sub TxtUsername_LostFocus(sender As Object, e As EventArgs)
        If String.IsNullOrWhiteSpace(txtUsername.Text) Then
            txtUsername.Text = "Username or Email"
            txtUsername.ForeColor = Color.LightGray
        End If
    End Sub

    ' Placeholder handlers for password textbox
    ' Placeholder handlers for password textbox
    Private Sub TxtPassword_GotFocus(sender As Object, e As EventArgs)
        If txtPassword.Text = "Password" Then
            txtPassword.Text = ""
            txtPassword.ForeColor = Color.White
            txtPassword.UseSystemPasswordChar = True ' Show asterisks when focused
        End If
    End Sub

    Private Sub TxtPassword_LostFocus(sender As Object, e As EventArgs)
        If String.IsNullOrWhiteSpace(txtPassword.Text) Then
            txtPassword.Text = "Password"
            txtPassword.ForeColor = Color.LightGray
            txtPassword.UseSystemPasswordChar = False ' Show plain text for placeholder
        End If
    End Sub

    ' Handle Enter key press in textboxes
    Private Sub TextBox_KeyPress(sender As Object, e As KeyPressEventArgs)
        If e.KeyChar = Chr(13) Then ' Enter key
            BtnLogIn_Click(sender, e)
        End If
    End Sub

    ' Save user credentials to registry
    Private Sub SaveCredentials(username As String, password As String, remember As Boolean)
        Try
            Using key As RegistryKey = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_PATH)
                If remember Then
                    key.SetValue(USERNAME_KEY, username)
                    key.SetValue(PASSWORD_KEY, EncryptPassword(password))
                    key.SetValue(REMEMBER_KEY, "True")
                Else
                    ' Clear saved credentials if not remembering
                    key.DeleteValue(USERNAME_KEY, False)
                    key.DeleteValue(PASSWORD_KEY, False)
                    key.SetValue(REMEMBER_KEY, "False")
                End If
            End Using
        Catch ex As Exception
            ' Silently handle registry errors
        End Try
    End Sub

    ' Encrypt and decrypt passwords
    Private Function EncryptPassword(password As String) As String
        Try
            Dim data As Byte() = Encoding.UTF8.GetBytes(password)
            Return Convert.ToBase64String(data)
        Catch ex As Exception
            Return password ' Fallback to plain text if encoding fails
        End Try
    End Function

    Private Function DecryptPassword(encodedPassword As String) As String
        Try
            Dim data As Byte() = Convert.FromBase64String(encodedPassword)
            Return Encoding.UTF8.GetString(data)
        Catch ex As Exception
            Return encodedPassword ' Fallback if decoding fails
        End Try
    End Function

    ' Login Button Click Event - Fixed version
    Private Sub BtnLogIn_Click(sender As Object, e As EventArgs)
        Try
            ' Validate input
            If Not ValidateLoginInput() Then
                Return
            End If

            ' Get username/email and password
            Dim usernameOrEmail As String = txtUsername.Text.Trim()
            Dim password As String = txtPassword.Text

            ' Authenticate user (using your existing method)
            Dim user As UserInfo = AuthenticateUser(usernameOrEmail, password)

            If user IsNot Nothing Then
                ' Save credentials if remember me is checked
                SaveCredentials(usernameOrEmail, password, chkRememberMe.Checked)

                ' Set the logged in user (keep your existing variable for compatibility)
                loggedInUser = user

                ' IMPORTANT: Set user session in SessionManager
                SessionManager.Instance.SetUserSession(user)

                ' Check user role and navigate accordingly
                If user.Role = "Admin" Then
                    MessageBox.Show("Welcome, Admin " & user.FirstName & "!", "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    ' Open AdminBookingInterface and hide this form
                    Dim adminForm As New AdminBookingInterface()
                    adminForm.Show()
                    Me.Hide()
                Else
                    ' Regular user login - SessionManager already populated GlobalSession
                    MessageBox.Show("Login successful! Welcome " & user.FirstName, "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    ' Navigate to homepage
                    Dim home As New TravelHomepageForm(user)
                    home.Show()
                    Me.Hide()
                End If
            Else
                MessageBox.Show("Invalid username/email or password. Please try again.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtPassword.Text = "Password"
                txtPassword.ForeColor = Color.LightGray
                txtPassword.UseSystemPasswordChar = False
                txtUsername.Focus()
            End If
        Catch ex As Exception
            MessageBox.Show($"Login error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Updated ReviewDialog constructor (no parameters needed now)
    Public Sub New(Optional bookingID As Integer = 0)

        ' Get user from SessionManager
        Dim currentUser As UserInfo = SessionManager.Instance.GetCurrentUserInfo()

        Console.WriteLine($"User from SessionManager is null: {currentUser Is Nothing}")

        If currentUser IsNot Nothing Then
            Console.WriteLine($"User.UserID: {currentUser.UserID}")
            Console.WriteLine($"User.FirstName: {currentUser.FirstName}")
            Console.WriteLine($"User.LastName: {currentUser.LastName}")
            Console.WriteLine($"User.Username: {currentUser.Username}")
            Console.WriteLine($"User.Email: {currentUser.Email}")
            Console.WriteLine($"User.Role: {currentUser.Role}")
        End If

        ' Validate user session
        If Not SessionManager.Instance.IsUserLoggedIn Then
            Console.WriteLine("ERROR: No valid user session found")
            MessageBox.Show("No user session found. Please log in to submit a review.", "Login Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            ' Set dialog result and close
            Me.DialogResult = DialogResult.Cancel
            Me.WindowState = FormWindowState.Minimized
            Me.Close()
            Return
        End If
    End Sub

    Private Function ValidateLoginInput() As Boolean
        ' Check if username field has placeholder text or is empty
        If txtUsername.Text = "Username or Email" OrElse String.IsNullOrWhiteSpace(txtUsername.Text) Then
            MessageBox.Show("Please enter your username or email.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtUsername.Focus()
            Return False
        End If

        ' Check if password field has placeholder text or is empty
        If txtPassword.Text = "Password" OrElse String.IsNullOrWhiteSpace(txtPassword.Text) Then
            MessageBox.Show("Please enter your password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtPassword.Focus()
            Return False
        End If

        Return True
    End Function

    ' Authenticate user against database - Fixed version
    Private Function AuthenticateUser(usernameOrEmail As String, password As String) As UserInfo
        Try
            If conn.State = ConnectionState.Closed Then
                conn.Open()
            End If

            ' Updated query to match the database schema
            Dim query As String = "SELECT UserID, Username, Email, Password, FirstName, LastName, Phone, Address, BirthDate, Gender, Role, IsActive, CreatedAt, UpdatedAt " &
                      "FROM Users WHERE (Username = @UsernameOrEmail OR Email = @UsernameOrEmail) AND IsActive = 1"

            Using dbcomm As New MySql.Data.MySqlClient.MySqlCommand(query, conn)
                dbcomm.Parameters.AddWithValue("@UsernameOrEmail", usernameOrEmail.Trim().ToLower())

                Using reader As MySql.Data.MySqlClient.MySqlDataReader = dbcomm.ExecuteReader()
                    If reader.Read() Then
                        Dim storedPassword As String = reader("Password").ToString()
                        Dim role As String = reader("Role").ToString()

                        ' Password verification logic
                        Dim passwordMatch As Boolean = False

                        If role = "Admin" Then
                            ' For admin users, check if it's plain text (legacy) or hashed
                            If storedPassword = password Then
                                ' Plain text match (legacy admin accounts)
                                passwordMatch = True
                            ElseIf VerifyPassword(password, storedPassword) Then
                                ' Hashed password match
                                passwordMatch = True
                            End If
                        Else
                            ' For regular users, always use hashed password verification
                            passwordMatch = VerifyPassword(password, storedPassword)
                        End If

                        If passwordMatch Then
                            Return MapUser(reader)
                        Else
                            MessageBox.Show("Invalid password.", "Authentication Failed",
                                      MessageBoxButtons.OK, MessageBoxIcon.Warning)
                            Return Nothing
                        End If
                    Else
                        MessageBox.Show("User not found or account is inactive.", "Authentication Failed",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        Return Nothing
                    End If
                End Using
            End Using

        Catch ex As MySql.Data.MySqlClient.MySqlException
            Select Case ex.Number
                Case 2003
                    MessageBox.Show("Cannot connect to database server. Please check your connection.",
                              "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Case Else
                    MessageBox.Show($"Database error: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Select
            Return Nothing

        Catch ex As Exception
            MessageBox.Show($"Authentication error: {ex.Message}", "Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return Nothing

        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If
        End Try
    End Function

    Private Function VerifyPassword(plainPassword As String, hashedPassword As String) As Boolean
        Try
            ' Generate hash of the plain password and compare with stored hash
            Dim inputHash As String = HashPassword(plainPassword)
            Return inputHash = hashedPassword
        Catch ex As Exception
            MessageBox.Show($"Password verification error: {ex.Message}", "Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    ' Updated MapUser function to match database schema
    Private Function MapUser(reader As MySql.Data.MySqlClient.MySqlDataReader) As UserInfo
        Try
            Dim user As New UserInfo()

            user.UserID = Convert.ToInt32(reader("UserID"))
            user.Username = reader("Username").ToString()
            user.Email = reader("Email").ToString()
            user.FirstName = reader("FirstName").ToString()
            user.LastName = reader("LastName").ToString()

            ' Handle nullable fields
            user.Phone = If(IsDBNull(reader("Phone")), String.Empty, reader("Phone").ToString())
            user.Address = If(IsDBNull(reader("Address")), String.Empty, reader("Address").ToString())
            user.Gender = If(IsDBNull(reader("Gender")), String.Empty, reader("Gender").ToString())

            ' Handle BirthDate (nullable DateTime)
            If Not IsDBNull(reader("BirthDate")) Then
                user.BirthDate = Convert.ToDateTime(reader("BirthDate"))
            End If

            user.Role = reader("Role").ToString()
            user.IsActive = Convert.ToBoolean(reader("IsActive"))
            user.CreatedAt = Convert.ToDateTime(reader("CreatedAt"))
            user.UpdatedAt = Convert.ToDateTime(reader("UpdatedAt"))

            Return user

        Catch ex As Exception
            MessageBox.Show($"Error mapping user data: {ex.Message}", "Mapping Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return Nothing
        End Try
    End Function

    ' Hash password using SHA256
    Private Function HashPassword(password As String) As String
        Try
            If String.IsNullOrEmpty(password) Then
                Throw New ArgumentException("Password cannot be null or empty")
            End If

            Using sha256Hash As SHA256 = SHA256.Create()
                Dim bytes As Byte() = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password))
                Dim builder As New StringBuilder(bytes.Length * 2)
                For Each b As Byte In bytes
                    builder.Append(b.ToString("x2"))
                Next
                Return builder.ToString()
            End Using

        Catch ex As Exception
            Throw New InvalidOperationException("Password hashing failed", ex)
        End Try
    End Function

    Private Sub BtnPackages_Click(sender As Object, e As EventArgs)
        Try
            Dim packagesForm As New PackagesWithoutUserForm()
            Me.Hide()
            packagesForm.ShowDialog()
            Me.Show()
        Catch ex As Exception
            MessageBox.Show($"Error opening Packages: {ex.Message}", "Navigation Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.Show()
        End Try
    End Sub

    Private Sub BtnAdventurerHome_Click(sender As Object, e As EventArgs)
        Try
            Dim adventurerForm As AdventurerHomeForm

            ' Check if user is logged in
            If IsUserLoggedIn() Then
                ' Pass both user and connection
                adventurerForm = New AdventurerHomeForm(loggedInUser)
            Else
                ' No user logged in - use parameterless constructor
                adventurerForm = New AdventurerHomeForm()
            End If

            Me.Hide()
            adventurerForm.ShowDialog()
            Me.Show()
        Catch ex As Exception
            MessageBox.Show("Error opening Adventurer Home: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.Show()
        End Try
    End Sub

    Private Sub BtnAboutUs_Click(sender As Object, e As EventArgs)
        Dim aboutUsForm As New AboutUsForm()
        aboutUsForm.ShowDialog()
    End Sub

    Private Sub BtnSignUp_Click(sender As Object, e As EventArgs)
        Try
            Dim signUpForm As New SignUpForm()
            Me.Hide()
            signUpForm.ShowDialog()
            Me.Show()
        Catch ex As Exception
            MessageBox.Show($"Error opening Sign Up: {ex.Message}", "Navigation Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.Show()
        End Try
    End Sub


    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)

        ' Update main content panel size
        If pnlMainContent IsNot Nothing Then
            pnlMainContent.Size = New Size(Me.Width - 100, 500)

            ' Recalculate centered content position
            Dim loginPanelStartX As Integer = Me.Width - 420
            Dim availableWidth As Integer = loginPanelStartX - 50
            Dim contentStartX As Integer = (availableWidth - 650) \ 2

            ' Update positions of main content elements
            For Each ctrl As Control In pnlMainContent.Controls
                If TypeOf ctrl Is Label Then
                    ctrl.Location = New Point(contentStartX, ctrl.Location.Y)
                ElseIf TypeOf ctrl Is PictureBox Then
                    ctrl.Location = New Point(contentStartX + 260, ctrl.Location.Y) ' Centered logo
                End If
            Next
        End If

        ' Update login panel position
        If pnlLogin IsNot Nothing Then
            pnlLogin.Location = New Point(Me.Width - 420, 175)
        End If

        ' Update top navigation
        If pnlTopNav IsNot Nothing Then
            pnlTopNav.Size = New Size(Me.Width, 70)
            ' Reposition right-aligned navigation buttons
            If btnPackages IsNot Nothing Then btnPackages.Location = New Point(630, 25)
            If btnAboutUs IsNot Nothing Then btnAboutUs.Location = New Point(750, 25)
            If btnAdventurerHome IsNot Nothing Then btnAdventurerHome.Location = New Point(850, 25)
        End If
    End Sub

    ' Property to get current logged-in user
    Public ReadOnly Property CurrentUser As UserInfo
        Get
            Return loggedInUser
        End Get
    End Property

    ' Method to check if user is logged in
    Public Function IsUserLoggedIn() As Boolean
        Return loggedInUser IsNot Nothing
    End Function

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            ' Test database connection on load
            If Not TestDatabaseConnection() Then
                MessageBox.Show("Warning: Cannot connect to database. Some features may not work.", "Database Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            MessageBox.Show("Form load error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function TestDatabaseConnection() As Boolean
        Try
            If conn.State = ConnectionState.Closed Then
                conn.Open()
            End If
            conn.Close()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function
End Class