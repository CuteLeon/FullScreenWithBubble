Imports System.Drawing.Drawing2D
Imports System.Runtime.InteropServices

Public Class 星星爆发咯
    '鼠标穿透窗体
    Private Declare Function GetWindowLong Lib "User32" Alias "GetWindowLongA" (ByVal hWnd As Integer, ByVal nIndex As Integer) As Integer
    Private Declare Function SetWindowLong Lib "User32" Alias "SetWindowLongA" (ByVal hWnd As Integer, ByVal nIndex As Integer,
                                                                                ByVal dwNewinteger As Integer) As Integer

    '这个就是我处理的场景咯
    Dim MyBitmap = New Bitmap(My.Computer.Screen.Bounds.Width, My.Computer.Screen.Bounds.Height)

    '随机绘制圆形区域
    Private Sub CreatCircle(ByRef SetBitmap As Bitmap, ByVal Radius As Integer, ByVal SetColor As Color)
        Dim CenterX, CenterY, IndexX, IndexY, Distance As Integer
        Dim ResultColor As Color
        '随机选取圆心位置
        CenterX = Int(VBMath.Rnd * SetBitmap.Width)
        CenterY = Int(VBMath.Rnd * SetBitmap.Height)

        '开始绘制咯
        For IndexX = CenterX - Radius To CenterX + Radius
            For IndexY = CenterY - Radius To CenterY + Radius
                If Not ((IndexX < 0 Or IndexX >= SetBitmap.Width) Or (IndexY < 0 Or IndexY >= SetBitmap.Height)) Then
                    '判断是否在半径之内
                    If Math.Sqrt((IndexX - CenterX) ^ 2 + (IndexY - CenterY) ^ 2) <= Radius Then
                        '如果在半径之内，那么距离到底是多少呢？
                        Distance = Math.Round(Math.Sqrt((IndexX - CenterX) ^ 2 + (IndexY - CenterY) ^ 2))

                        '混合两个可能重叠的带Alpha通道的颜色
                        'ResultColor = MixAlphaColor(Color.FromArgb(Int(255 * (1 - Distance ^ 2 / Radius ^ 2)), SetColor), SetBitmap.GetPixel(IndexX, IndexY))
                        ResultColor = Color.FromArgb(Int(255 * (1 - Distance ^ 2 / (1.0 * Radius) ^ 2)), SetColor)      '不混合

                        '好咯，我要储存到变量上咯 
                        SetBitmap.SetPixel(IndexX, IndexY, ResultColor)
                    End If
                End If
            Next
        Next
    End Sub

    '混合两个带Alpha通道的颜色
    Private Function MixAlphaColor(ByVal Color1 As Color, ByVal Color2 As Color) As Color
        '混合阿尔法透明度
        Dim TempAlpha As Integer = CLng(Color1.A) + CLng(Color2.A) - CLng(Color1.A) * CLng(Color2.A) \ 255
        '混合RGB颜色
        Dim TempRed As Integer = (CLng(Color1.R) * CLng(Color1.A) + CLng(Color2.R) * CLng(Color2.A) _
                                  - (CLng(Color1.R) * CLng(Color1.A) * CLng(Color2.A)) \ 255) \ 255
        Dim TempGreen As Integer = (CLng(Color1.G) * CLng(Color1.A) + CLng(Color2.G) * CLng(Color2.A) _
                                    - (CLng(Color1.G) * CLng(Color1.A) * CLng(Color2.A)) / 255) \ 255
        Dim TempBlue As Integer = (CLng(Color1.B) * CLng(Color1.A) + CLng(Color2.B) * CLng(Color2.A) _
                                   - (CLng(Color1.B) * CLng(Color1.A) * CLng(Color2.A)) / 255) \ 255
        '返回混合后的颜色
        Return Color.FromArgb(TempAlpha, TempRed, TempGreen, TempBlue)
    End Function

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Static MaxRadius As Integer = 50
        MaxRadius = IIf(MaxRadius = 600, 600, MaxRadius + 1)

        '窗体要置顶
        Me.TopMost = True
        '随机产生三个短整型组成Color
        Dim TempColor As Color = Color.FromArgb(VBMath.Rnd * 255, VBMath.Rnd * 255, VBMath.Rnd * 255)
        '调用过程绘圆
        CreatCircle(MyBitmap, Int(VBMath.Rnd * MaxRadius \ 10) + 1, TempColor)
        '把场景绘制到屏幕
        SetAlphaPicture(Me, MyBitmap)
    End Sub

    Public Sub SetAlphaPicture(ByVal AlphaWindow As Form, ByVal AlphaPicture As Bitmap)
        '在内存中创建与当前屏幕兼容的DC 
        Dim hDC1 As IntPtr = Win32.GetDC(IntPtr.Zero)
        Dim hDC2 As IntPtr = Win32.CreateCompatibleDC(hDC1)
        Dim hBitmap1 As IntPtr = IntPtr.Zero
        Dim hBitmap2 As IntPtr = IntPtr.Zero

        Try
            hBitmap1 = AlphaPicture.GetHbitmap(Color.FromArgb(0))
            hBitmap2 = Win32.SelectObject(hDC2, hBitmap1)

            Dim blend As New Win32.BLENDFUNCTION()
            With blend
                .BlendOp = Win32.AC_SRC_OVER
                .BlendFlags = 0
                .AlphaFormat = Win32.AC_SRC_ALPHA
                .SourceConstantAlpha = True
            End With

            Call Win32.UpdateLayeredWindow(AlphaWindow.Handle, hDC1, New Win32.Point(AlphaWindow.Left,
                                                    AlphaWindow.Top), New Win32.Size(AlphaPicture.Width, AlphaPicture.Height),
                                                    hDC2, New Win32.Point(0, 0), 0, blend, Win32.ULW_ALPHA)

        Finally
            Call Win32.ReleaseDC(IntPtr.Zero, hDC1)
            If hBitmap1 <> IntPtr.Zero Then
                Call Win32.SelectObject(hDC2, hBitmap2)
                Call Win32.DeleteObject(hBitmap1)
            End If
            Call Win32.DeleteDC(hDC2)
        End Try
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        '窗体覆盖全屏
        With Me
            .Left = 0
            .Top = 0
            .Width = My.Computer.Screen.Bounds.Width
            .Height = My.Computer.Screen.Bounds.Height
        End With
        '窗体添加鼠标穿透功能，不会影响用户的鼠标操作
        SetWindowLong(Me.Handle, -20, GetWindowLong(Me.Handle, -20) Or &H80000 Or &H20&)
    End Sub

    Protected Overloads Overrides ReadOnly Property CreateParams() As CreateParams
        Get
            If Not DesignMode Then
                Dim cp As CreateParams = MyBase.CreateParams
                cp.ExStyle = cp.ExStyle Or Win32.WS_EX_LAYERED
                Return cp
            Else
                Return MyBase.CreateParams
            End If
        End Get
    End Property

End Class

Public Class Win32

    Private Const ULW_COLORKEY As Int32 = &H1
    Public Const ULW_ALPHA As Int32 = &H2
    Private Const ULW_OPAQUE As Int32 = &H4
    Public Const WS_EX_LAYERED As Int32 = &H80000
    Public Const AC_SRC_OVER As Byte = &H0
    Public Const AC_SRC_ALPHA As Byte = &H1

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure Size
        Private cx As Int32
        Private cy As Int32

        Public Sub New(ByVal cx As Int32, ByVal cy As Int32)
            Me.cx = cx
            Me.cy = cy
        End Sub
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure Point
        Private x As Int32
        Private y As Int32

        Public Sub New(ByVal x As Int32, ByVal y As Int32)
            Me.x = x
            Me.y = y
        End Sub
    End Structure

    <StructLayout(LayoutKind.Sequential, Pack:=1)> _
    Public Structure BLENDFUNCTION
        Public BlendOp As Byte
        Public BlendFlags As Byte
        Public SourceConstantAlpha As Byte
        Public AlphaFormat As Byte
    End Structure

    '该函数检索一指定窗口的客户区域或整个屏幕的显示设备上下文环境的句柄，以后可以在GDI函数中使用该句柄来在设备上下文环境中绘图。 
    Public Declare Auto Function GetDC Lib "user32.dll" (ByVal hWnd As IntPtr) As IntPtr
    '该函数创建一个与指定设备兼容的内存设备上下文环境（DC）。通过GetDc()获取的HDC直接与相关设备沟通，而本函数创建的DC，则是与内存中的一个表面相关联。 
    Public Declare Auto Function CreateCompatibleDC Lib "gdi32.dll" (ByVal hDC As IntPtr) As IntPtr
    '该函数选择一对象到指定的设备上下文环境中，该新对象替换先前的相同类型的对象。 
    Public Declare Auto Function SelectObject Lib "gdi32.dll" (ByVal hDC As IntPtr, ByVal hObject As IntPtr) As IntPtr
    '该函数更新一个分层的窗口的位置，大小，形状，内容和半透明度。 
    Public Declare Auto Function UpdateLayeredWindow Lib "user32.dll" (ByVal hwnd As IntPtr, ByVal hdcDst As IntPtr, ByRef pptDst As Point, ByRef psize As Size, ByVal hdcSrc As IntPtr, ByRef pprSrc As Point, ByVal crKey As Int32, ByRef pblend As BLENDFUNCTION, ByVal dwFlags As Int32) As Boolean
    '该函数释放设备上下文环境（DC）供其他应用程序使用。函数的效果与设备上下文环境类型有关。它只释放公用的和设备上下文环境，对于类或私有的则无效。 
    Public Declare Auto Function ReleaseDC Lib "user32.dll" (ByVal hWnd As IntPtr, ByVal hDC As IntPtr) As Integer
    '该函数删除一个逻辑笔、画笔、字体、位图、区域或者调色板，释放所有与该对象有关的系统资源，在对象被删除之后，指定的句柄也就失效了。 
    Public Declare Auto Function DeleteObject Lib "gdi32.dll" (ByVal hObject As IntPtr) As Boolean
    '该函数删除指定的设备上下文环境（DC）。 
    Public Declare Auto Function DeleteDC Lib "gdi32.dll" (ByVal hdc As IntPtr) As Boolean
End Class
