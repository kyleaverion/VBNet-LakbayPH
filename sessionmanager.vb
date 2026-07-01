Imports MySql.Data.MySqlClient
Imports projectv2.MainForm

Public Class SessionManager
    Private Shared _instance As SessionManager
    Private Shared ReadOnly _lock As New Object()

    ' Private constructor to prevent direct instantiation
    Private Sub New()
    End Sub

    ' Singleton pattern to ensure only one instance exists
    Public Shared ReadOnly Property Instance() As SessionManager
        Get
            If _instance Is Nothing Then
                SyncLock _lock
                    If _instance Is Nothing Then
                        _instance = New SessionManager()
                    End If
                End SyncLock
            End If
            Return _instance
        End Get
    End Property

    ' Current user session - using your existing UserInfo class
    Private _currentUser As UserInfo

    ' Property to get/set current user
    Public Property CurrentUser As UserInfo
        Get
            Return _currentUser
        End Get
        Set(value As UserInfo)
            _currentUser = value
            If _currentUser IsNot Nothing Then
                Console.WriteLine($"SessionManager: User set - ID: {_currentUser.UserID}, Username: {_currentUser.Username}, Role: {_currentUser.Role}")
            Else
                Console.WriteLine("SessionManager: User cleared")
            End If
        End Set
    End Property

    ' Check if user is logged in
    Public ReadOnly Property IsUserLoggedIn As Boolean
        Get
            Return _currentUser IsNot Nothing AndAlso _currentUser.UserID > 0
        End Get
    End Property

    ' Check if current user is admin
    Public ReadOnly Property IsAdmin As Boolean
        Get
            Return _currentUser IsNot Nothing AndAlso _currentUser.Role = "Admin"
        End Get
    End Property

    ' Set user session (called from your existing login method)
    Public Sub SetUserSession(user As UserInfo)
        _currentUser = user

        ' Also populate GlobalSession for backward compatibility
        If user IsNot Nothing Then
            Try
                GlobalSession.UserID = user.UserID
                GlobalSession.Username = user.Username
                GlobalSession.Email = user.Email
                GlobalSession.FirstName = user.FirstName
                GlobalSession.LastName = user.LastName
                GlobalSession.Role = user.Role
                Console.WriteLine($"SessionManager: Session set for user {user.Username} (ID: {user.UserID})")
            Catch ex As Exception
                Console.WriteLine($"Error setting GlobalSession: {ex.Message}")
            End Try
        Else
            ' Clear GlobalSession
            Try
                GlobalSession.UserID = 0
                GlobalSession.Username = ""
                GlobalSession.Email = ""
                GlobalSession.FirstName = ""
                GlobalSession.LastName = ""
                GlobalSession.Role = ""
            Catch ex As Exception
                Console.WriteLine($"Error clearing GlobalSession: {ex.Message}")
            End Try
        End If
    End Sub

    ' Logout method
    Public Sub Logout()
        Console.WriteLine($"Logging out user: {_currentUser?.Username}")
        _currentUser = Nothing

        ' Clear GlobalSession
        Try
            GlobalSession.UserID = 0
            GlobalSession.Username = ""
            GlobalSession.Email = ""
            GlobalSession.FirstName = ""
            GlobalSession.LastName = ""
            GlobalSession.Role = ""
        Catch ex As Exception
            Console.WriteLine($"Error clearing GlobalSession during logout: {ex.Message}")
        End Try
    End Sub

    ' Validate current session
    Public Function ValidateSession() As Boolean
        If _currentUser Is Nothing Then
            Console.WriteLine("Session validation failed: No user logged in")
            Return False
        End If

        If _currentUser.UserID <= 0 Then
            Console.WriteLine($"Session validation failed: Invalid user ID {_currentUser.UserID}")
            Return False
        End If

        ' Optional: Validate against database to ensure user still exists
        Try
            Return ValidateUserInDatabase(_currentUser.UserID)
        Catch ex As Exception
            Console.WriteLine($"Session validation error: {ex.Message}")
            Return False
        End Try
    End Function

    ' Validate user exists in database
    Private Function ValidateUserInDatabase(userID As Integer) As Boolean
        Try
            Dim connectionString As String = "Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;"
            Using connection As New MySqlConnection(connectionString)
                connection.Open()
                Dim query As String = "SELECT COUNT(*) FROM Users WHERE UserID = @userID"
                Using command As New MySqlCommand(query, connection)
                    command.Parameters.AddWithValue("@userID", userID)
                    Dim count As Integer = Convert.ToInt32(command.ExecuteScalar())
                    Return count > 0
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine($"Database validation error: {ex.Message}")
            Return False
        End Try
    End Function

    ' Get user info safely
    Public Function GetCurrentUserInfo() As UserInfo
        If ValidateSession() Then
            Return _currentUser
        End If
        Return Nothing
    End Function

    ' Refresh user data from database
    Public Function RefreshUserData() As Boolean
        If _currentUser Is Nothing OrElse _currentUser.UserID <= 0 Then
            Return False
        End If

        Try
            Dim connectionString As String = "Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;"
            Using connection As New MySqlConnection(connectionString)
                connection.Open()
                Dim query As String = "SELECT UserID, FirstName, LastName, Username, Email, Role FROM Users WHERE UserID = @userID"
                Using command As New MySqlCommand(query, connection)
                    command.Parameters.AddWithValue("@userID", _currentUser.UserID)

                    Using reader As MySqlDataReader = command.ExecuteReader()
                        If reader.Read() Then
                            _currentUser.FirstName = reader.GetString("FirstName")
                            _currentUser.LastName = reader.GetString("LastName")
                            _currentUser.Username = reader.GetString("Username")
                            _currentUser.Email = reader.GetString("Email")
                            _currentUser.Role = reader.GetString("Role")

                            ' Update GlobalSession as well
                            SetUserSession(_currentUser)
                            Return True
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine($"Error refreshing user data: {ex.Message}")
        End Try

        Return False
    End Function

    Friend Sub ClearSession()
        Throw New NotImplementedException()
    End Sub
End Class
