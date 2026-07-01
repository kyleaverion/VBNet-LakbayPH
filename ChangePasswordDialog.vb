Public Class ChangePasswordDialog
    Inherits Form

    Private WithEvents txtCurrentPassword As TextBox
    Private WithEvents txtNewPassword As TextBox
    Private WithEvents txtConfirmPassword As TextBox
    Private WithEvents btnSave As Button
    Private WithEvents btnCancel As Button
    Private WithEvents lblCurrentPassword As Label
    Private WithEvents lblNewPassword As Label
    Private WithEvents lblConfirmPassword As Label
    Private WithEvents lblTitle As Label

    Private _userId As Integer
    Private connectionString As String = "Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;"

    Public Sub New(userId As Integer)
        _userId = userId
        InitializeComponentDialogs()
    End Sub

    Private Sub InitializeComponentDialogs()
        Me.Text = "Change Password"
        Me.Size = New Size(400, 400)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.BackColor = Color.FromArgb(240, 248, 255)
        Me.Font = New Font("Segoe UI", 10)

        ' Title
        lblTitle = New Label()
        lblTitle.Text = "Change Password"
        lblTitle.Font = New Font("Segoe UI", 14, FontStyle.Bold)
        lblTitle.ForeColor = Color.FromArgb(41, 98, 255)
        lblTitle.Location = New Point(20, 20)
        lblTitle.Size = New Size(200, 30)
        Me.Controls.Add(lblTitle)

        ' Current Password
        lblCurrentPassword = New Label()
        lblCurrentPassword.Text = "Current Password:"
        lblCurrentPassword.Location = New Point(20, 70)
        lblCurrentPassword.Size = New Size(150, 25)
        Me.Controls.Add(lblCurrentPassword)

        txtCurrentPassword = New TextBox()
        txtCurrentPassword.Location = New Point(20, 100)
        txtCurrentPassword.Size = New Size(340, 30)
        txtCurrentPassword.UseSystemPasswordChar = True
        txtCurrentPassword.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(txtCurrentPassword)

        ' New Password
        lblNewPassword = New Label()
        lblNewPassword.Text = "New Password:"
        lblNewPassword.Location = New Point(20, 140)
        lblNewPassword.Size = New Size(150, 25)
        Me.Controls.Add(lblNewPassword)

        txtNewPassword = New TextBox()
        txtNewPassword.Location = New Point(20, 170)
        txtNewPassword.Size = New Size(340, 30)
        txtNewPassword.UseSystemPasswordChar = True
        txtNewPassword.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(txtNewPassword)

        ' Confirm Password
        lblConfirmPassword = New Label()
        lblConfirmPassword.Text = "Confirm New Password:"
        lblConfirmPassword.Location = New Point(20, 210)
        lblConfirmPassword.Size = New Size(180, 25)
        Me.Controls.Add(lblConfirmPassword)

        txtConfirmPassword = New TextBox()
        txtConfirmPassword.Location = New Point(20, 240)
        txtConfirmPassword.Size = New Size(340, 30)
        txtConfirmPassword.UseSystemPasswordChar = True
        txtConfirmPassword.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(txtConfirmPassword)

        ' Buttons
        btnSave = New Button()
        btnSave.Text = "Change Password"
        btnSave.Location = New Point(80, 290)
        btnSave.Size = New Size(130, 35)
        btnSave.BackColor = Color.FromArgb(41, 98, 255)
        btnSave.ForeColor = Color.White
        btnSave.FlatStyle = FlatStyle.Flat
        btnSave.FlatAppearance.BorderSize = 0
        AddHandler btnSave.Click, AddressOf btnSave_Click
        Me.Controls.Add(btnSave)

        btnCancel = New Button()
        btnCancel.Text = "Cancel"
        btnCancel.Location = New Point(220, 290)
        btnCancel.Size = New Size(80, 35)
        btnCancel.BackColor = Color.Gray
        btnCancel.ForeColor = Color.White
        btnCancel.FlatStyle = FlatStyle.Flat
        btnCancel.FlatAppearance.BorderSize = 0
        btnCancel.DialogResult = DialogResult.Cancel
        Me.Controls.Add(btnCancel)

        Me.AcceptButton = btnSave
        Me.CancelButton = btnCancel
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs)
        If ValidatePasswords() Then
            If ChangePassword() Then
                Me.DialogResult = DialogResult.OK
                Me.Close()
            End If
        End If
    End Sub

    Private Function ValidatePasswords() As Boolean
        ' Check if all fields are filled
        If String.IsNullOrWhiteSpace(txtCurrentPassword.Text) Then
            MessageBox.Show("Please enter your current password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtCurrentPassword.Focus()
            Return False
        End If

        If String.IsNullOrWhiteSpace(txtNewPassword.Text) Then
            MessageBox.Show("Please enter a new password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtNewPassword.Focus()
            Return False
        End If

        If String.IsNullOrWhiteSpace(txtConfirmPassword.Text) Then
            MessageBox.Show("Please confirm your new password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtConfirmPassword.Focus()
            Return False
        End If

        ' Check if new passwords match
        If txtNewPassword.Text.Trim() <> txtConfirmPassword.Text.Trim() Then
            MessageBox.Show("New passwords do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtConfirmPassword.SelectAll()
            txtConfirmPassword.Focus()
            Return False
        End If

        ' Check password strength
        If txtNewPassword.Text.Trim().Length < 6 Then
            MessageBox.Show("New password must be at least 6 characters long.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtNewPassword.SelectAll()
            txtNewPassword.Focus()
            Return False
        End If

        ' Additional password strength checks
        If txtNewPassword.Text.Trim().Length > 50 Then
            MessageBox.Show("Password cannot exceed 50 characters.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtNewPassword.SelectAll()
            txtNewPassword.Focus()
            Return False
        End If

        ' Check for common weak passwords
        Dim weakPasswords As String() = {"123456", "password", "123456789", "qwerty", "abc123", "password123"}
        If weakPasswords.Contains(txtNewPassword.Text.Trim().ToLower()) Then
            MessageBox.Show("Please choose a stronger password. Avoid common passwords.", "Weak Password", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtNewPassword.SelectAll()
            txtNewPassword.Focus()
            Return False
        End If

        Return True
    End Function

    Private Function ChangePassword() As Boolean
        Dim transaction As MySql.Data.MySqlClient.MySqlTransaction = Nothing

        Try
            Using conn As New MySql.Data.MySqlClient.MySqlConnection(connectionString)
                conn.Open()
                transaction = conn.BeginTransaction()

                ' First verify current password from database
                Dim verifyQuery As String = "SELECT Password FROM Users WHERE UserID = @UserID"
                Using verifyCmd As New MySql.Data.MySqlClient.MySqlCommand(verifyQuery, conn, transaction)
                    verifyCmd.Parameters.AddWithValue("@UserID", _userId)

                    Dim storedPassword As Object = verifyCmd.ExecuteScalar()
                    If storedPassword Is Nothing Then
                        MessageBox.Show("User not found in database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        transaction.Rollback()
                        Return False
                    End If

                    ' Get the stored password hash from database
                    Dim currentPasswordInDB As String = If(IsDBNull(storedPassword), "", storedPassword.ToString())

                    ' Hash the entered current password and compare with stored hash
                    Dim hashedCurrentPassword As String = HashPassword(txtCurrentPassword.Text.Trim())

                    If currentPasswordInDB <> hashedCurrentPassword Then
                        MessageBox.Show("Current password is incorrect. Please verify your current password.", "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        txtCurrentPassword.SelectAll()
                        txtCurrentPassword.Focus()
                        transaction.Rollback()
                        Return False
                    End If
                End Using

                ' Check if new password is different from current password
                If txtCurrentPassword.Text = txtNewPassword.Text Then
                    MessageBox.Show("New password must be different from current password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    txtNewPassword.Focus()
                    transaction.Rollback()
                    Return False
                End If

                ' Hash the new password before storing in database
                Dim hashedNewPassword As String = HashPassword(txtNewPassword.Text.Trim())

                ' Update password in database with the hashed version
                Dim updateQuery As String = "UPDATE Users SET Password = @NewPassword, UpdatedAt = NOW() WHERE UserID = @UserID"
                Using updateCmd As New MySql.Data.MySqlClient.MySqlCommand(updateQuery, conn, transaction)
                    updateCmd.Parameters.AddWithValue("@NewPassword", hashedNewPassword)
                    updateCmd.Parameters.AddWithValue("@UserID", _userId)

                    Dim rowsAffected As Integer = updateCmd.ExecuteNonQuery()
                    If rowsAffected > 0 Then
                        ' Verify the update was successful by checking the new password hash
                        Dim verifyUpdateQuery As String = "SELECT Password FROM Users WHERE UserID = @UserID"
                        Using verifyUpdateCmd As New MySql.Data.MySqlClient.MySqlCommand(verifyUpdateQuery, conn, transaction)
                            verifyUpdateCmd.Parameters.AddWithValue("@UserID", _userId)

                            Dim updatedPassword As Object = verifyUpdateCmd.ExecuteScalar()
                            If updatedPassword IsNot Nothing AndAlso updatedPassword.ToString() = hashedNewPassword Then
                                transaction.Commit()
                                MessageBox.Show("Password changed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                                Return True
                            Else
                                transaction.Rollback()
                                MessageBox.Show("Password update verification failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                Return False
                            End If
                        End Using
                    Else
                        transaction.Rollback()
                        MessageBox.Show("Failed to update password. No rows were affected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Return False
                    End If
                End Using
            End Using

        Catch ex As MySql.Data.MySqlClient.MySqlException
            If transaction IsNot Nothing Then
                Try
                    transaction.Rollback()
                Catch rollbackEx As Exception
                    MessageBox.Show($"Error during rollback: {rollbackEx.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
            MessageBox.Show($"Database error while changing password: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        Catch ex As Exception
            If transaction IsNot Nothing Then
                Try
                    transaction.Rollback()
                Catch rollbackEx As Exception
                    MessageBox.Show($"Error during rollback: {rollbackEx.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
            MessageBox.Show($"Unexpected error while changing password: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        Finally
            If transaction IsNot Nothing Then
                transaction.Dispose()
            End If
        End Try
    End Function

    ' Make sure you have the same HashPassword function in this form/class
    Private Function HashPassword(password As String) As String
        Try
            ' Input validation
            If String.IsNullOrEmpty(password) Then
                Throw New ArgumentException("Password cannot be null or empty")
            End If
            Using sha256Hash As System.Security.Cryptography.SHA256 = System.Security.Cryptography.SHA256.Create()
                ' ComputeHash - returns byte array
                Dim bytes As Byte() = sha256Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password))
                ' Convert byte array to a string using StringBuilder for better performance
                Dim builder As New System.Text.StringBuilder(bytes.Length * 2)
                For Each b As Byte In bytes
                    builder.Append(b.ToString("x2"))
                Next
                Return builder.ToString()
            End Using
        Catch ex As Exception
            ' Log the error for debugging purposes
            MessageBox.Show($"Error hashing password: {ex.Message}", "Hash Error",
                  MessageBoxButtons.OK, MessageBoxIcon.Error)
            Throw New InvalidOperationException("Password hashing failed", ex)
        End Try
    End Function

    Private Sub ChangePasswordDialog_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class