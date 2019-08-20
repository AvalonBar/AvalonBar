Imports System.Windows.Controls
Imports System.Windows.Media.Imaging

Public Class TileInfo
    Inherits Attribute
    Public ReadOnly Name As String
    Public ReadOnly hasFlyout As Boolean
    Public ReadOnly hasOptions As Boolean
    Sub New(ByVal Name As String, ByVal hasFlyout As Boolean, ByVal hasOptions As Boolean)
        Me.Name = Name
        Me.hasFlyout = hasFlyout
        Me.hasOptions = hasOptions
    End Sub
End Class

Public MustInherit Class BaseTile
    Public Event CaptionChanged(ByVal value As String)
    Public Event IconChanged(ByVal value As BitmapImage)
    Public Event ShowFlyoutEvent()
    Public Event ShowOptionsEvent()
    Private _Caption As String
    Private _Icon As BitmapImage
    Private _IsMinimized As Boolean
    Private _FlyoutContent As UserControl
    Private _OptionsContent As UserControl
    Public Property Caption() As String
        Get
            Return _Caption
        End Get
        Set(ByVal value As String)
            _Caption = value
            RaiseEvent CaptionChanged(value)
        End Set
    End Property
    Public Property IsMinimized() As Boolean
        Get
            Return _IsMinimized
        End Get
        Set(ByVal value As Boolean)
            _IsMinimized = value
        End Set
    End Property
    Public Property Icon() As BitmapImage
        Get
            Return _Icon
        End Get
        Set(ByVal value As BitmapImage)
            _Icon = value
            RaiseEvent IconChanged(value)
        End Set
    End Property
    Public Property FlyoutContent() As UserControl
        Get
            Return _FlyoutContent
        End Get
        Set(ByVal value As UserControl)
            _FlyoutContent = value
        End Set
    End Property
    Public Property OptionsContent() As UserControl
        Get
            Return _OptionsContent
        End Get
        Set(ByVal value As UserControl)
            _OptionsContent = value
        End Set
    End Property
    Public MustOverride Function Load() As UserControl
    Public Overridable Sub Unload()

    End Sub
    Public Overridable Sub ChangeSide(ByVal side As Integer)

    End Sub
    Public Overridable Sub ChangeLocale(ByVal Locale As String)

    End Sub
    Public Overridable Sub ChangeTheme(ByVal Theme As String)

    End Sub
    Public Overridable Sub Minimized()

    End Sub
    Public Overridable Sub Unminimized()

    End Sub
    Public Overridable Sub ShowFlyout()
        RaiseEvent ShowFlyoutEvent()
    End Sub
    Public Overridable Sub ShowOptions()
        RaiseEvent ShowOptionsEvent()
    End Sub
End Class