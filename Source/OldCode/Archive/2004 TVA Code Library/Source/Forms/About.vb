'Author: Pinal Patel
'Created: 06-01-05
'Modified: 06-01-05
'Description: This is ESO's standard "About" windows form.



'Namespaces used.
Imports TVA.Shared.Assembly

Namespace Forms

    Public Class About
        Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

        Public Sub New()
            MyBase.New()

            'This call is required by the Windows Form Designer.
            InitializeComponent()

            'Add any initialization after the InitializeComponent() call

        End Sub

        'Form overrides dispose to clean up the component list.
        Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing Then
                If Not (components Is Nothing) Then
                    components.Dispose()
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        Friend WithEvents Panel1 As System.Windows.Forms.Panel
        Friend WithEvents Panel2 As System.Windows.Forms.Panel
        Friend WithEvents Panel3 As System.Windows.Forms.Panel
        Friend WithEvents btnOK As System.Windows.Forms.Button
        Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
        Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
        Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
        Friend WithEvents lvApplication As System.Windows.Forms.ListView
        Friend WithEvents lvchApplication1 As System.Windows.Forms.ColumnHeader
        Friend WithEvents lvchApplication2 As System.Windows.Forms.ColumnHeader
        Friend WithEvents lvAssemblies As System.Windows.Forms.ListView
        Friend WithEvents lvchAssemblies1 As System.Windows.Forms.ColumnHeader
        Friend WithEvents lvchAssemblies2 As System.Windows.Forms.ColumnHeader
        Friend WithEvents cboAssemblies As System.Windows.Forms.ComboBox
        Friend WithEvents llHomePage As System.Windows.Forms.LinkLabel
        Friend WithEvents picLogo As System.Windows.Forms.PictureBox
        <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
            Dim resources As System.Resources.ResourceManager = New System.Resources.ResourceManager(GetType(About))
            Me.Panel1 = New System.Windows.Forms.Panel
            Me.btnOK = New System.Windows.Forms.Button
            Me.Panel2 = New System.Windows.Forms.Panel
            Me.Panel3 = New System.Windows.Forms.Panel
            Me.TabControl1 = New System.Windows.Forms.TabControl
            Me.TabPage1 = New System.Windows.Forms.TabPage
            Me.lvApplication = New System.Windows.Forms.ListView
            Me.lvchApplication1 = New System.Windows.Forms.ColumnHeader
            Me.lvchApplication2 = New System.Windows.Forms.ColumnHeader
            Me.TabPage2 = New System.Windows.Forms.TabPage
            Me.lvAssemblies = New System.Windows.Forms.ListView
            Me.lvchAssemblies1 = New System.Windows.Forms.ColumnHeader
            Me.lvchAssemblies2 = New System.Windows.Forms.ColumnHeader
            Me.cboAssemblies = New System.Windows.Forms.ComboBox
            Me.llHomePage = New System.Windows.Forms.LinkLabel
            Me.picLogo = New System.Windows.Forms.PictureBox
            Me.Panel1.SuspendLayout()
            Me.Panel2.SuspendLayout()
            Me.Panel3.SuspendLayout()
            Me.TabControl1.SuspendLayout()
            Me.TabPage1.SuspendLayout()
            Me.TabPage2.SuspendLayout()
            Me.SuspendLayout()
            '
            'Panel1
            '
            Me.Panel1.Controls.Add(Me.btnOK)
            Me.Panel1.Dock = System.Windows.Forms.DockStyle.Bottom
            Me.Panel1.DockPadding.All = 10
            Me.Panel1.Location = New System.Drawing.Point(0, 323)
            Me.Panel1.Name = "Panel1"
            Me.Panel1.Size = New System.Drawing.Size(444, 45)
            Me.Panel1.TabIndex = 0
            '
            'btnOK
            '
            Me.btnOK.Location = New System.Drawing.Point(184, 12)
            Me.btnOK.Name = "btnOK"
            Me.btnOK.TabIndex = 0
            Me.btnOK.Text = "OK"
            '
            'Panel2
            '
            Me.Panel2.Controls.Add(Me.picLogo)
            Me.Panel2.Controls.Add(Me.llHomePage)
            Me.Panel2.Dock = System.Windows.Forms.DockStyle.Top
            Me.Panel2.DockPadding.Left = 10
            Me.Panel2.DockPadding.Right = 10
            Me.Panel2.DockPadding.Top = 10
            Me.Panel2.Location = New System.Drawing.Point(0, 0)
            Me.Panel2.Name = "Panel2"
            Me.Panel2.Size = New System.Drawing.Size(444, 70)
            Me.Panel2.TabIndex = 1
            '
            'Panel3
            '
            Me.Panel3.Controls.Add(Me.TabControl1)
            Me.Panel3.Dock = System.Windows.Forms.DockStyle.Fill
            Me.Panel3.DockPadding.Left = 10
            Me.Panel3.DockPadding.Right = 10
            Me.Panel3.DockPadding.Top = 10
            Me.Panel3.Location = New System.Drawing.Point(0, 70)
            Me.Panel3.Name = "Panel3"
            Me.Panel3.Size = New System.Drawing.Size(444, 253)
            Me.Panel3.TabIndex = 2
            '
            'TabControl1
            '
            Me.TabControl1.Controls.Add(Me.TabPage1)
            Me.TabControl1.Controls.Add(Me.TabPage2)
            Me.TabControl1.Dock = System.Windows.Forms.DockStyle.Fill
            Me.TabControl1.Location = New System.Drawing.Point(10, 10)
            Me.TabControl1.Name = "TabControl1"
            Me.TabControl1.SelectedIndex = 0
            Me.TabControl1.Size = New System.Drawing.Size(424, 243)
            Me.TabControl1.TabIndex = 0
            '
            'TabPage1
            '
            Me.TabPage1.Controls.Add(Me.lvApplication)
            Me.TabPage1.Location = New System.Drawing.Point(4, 22)
            Me.TabPage1.Name = "TabPage1"
            Me.TabPage1.Size = New System.Drawing.Size(416, 217)
            Me.TabPage1.TabIndex = 0
            Me.TabPage1.Text = "Application"
            '
            'lvApplication
            '
            Me.lvApplication.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.lvchApplication1, Me.lvchApplication2})
            Me.lvApplication.Dock = System.Windows.Forms.DockStyle.Fill
            Me.lvApplication.FullRowSelect = True
            Me.lvApplication.Location = New System.Drawing.Point(0, 0)
            Me.lvApplication.MultiSelect = False
            Me.lvApplication.Name = "lvApplication"
            Me.lvApplication.Size = New System.Drawing.Size(416, 217)
            Me.lvApplication.TabIndex = 0
            Me.lvApplication.View = System.Windows.Forms.View.Details
            '
            'lvchApplication1
            '
            Me.lvchApplication1.Text = "Key"
            Me.lvchApplication1.Width = 100
            '
            'lvchApplication2
            '
            Me.lvchApplication2.Text = "Value"
            Me.lvchApplication2.Width = 300
            '
            'TabPage2
            '
            Me.TabPage2.Controls.Add(Me.lvAssemblies)
            Me.TabPage2.Controls.Add(Me.cboAssemblies)
            Me.TabPage2.Location = New System.Drawing.Point(4, 22)
            Me.TabPage2.Name = "TabPage2"
            Me.TabPage2.Size = New System.Drawing.Size(416, 231)
            Me.TabPage2.TabIndex = 1
            Me.TabPage2.Text = "Assemblies"
            '
            'lvAssemblies
            '
            Me.lvAssemblies.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.lvchAssemblies1, Me.lvchAssemblies2})
            Me.lvAssemblies.Dock = System.Windows.Forms.DockStyle.Fill
            Me.lvAssemblies.FullRowSelect = True
            Me.lvAssemblies.Location = New System.Drawing.Point(0, 20)
            Me.lvAssemblies.MultiSelect = False
            Me.lvAssemblies.Name = "lvAssemblies"
            Me.lvAssemblies.Size = New System.Drawing.Size(416, 211)
            Me.lvAssemblies.TabIndex = 2
            Me.lvAssemblies.View = System.Windows.Forms.View.Details
            '
            'lvchAssemblies1
            '
            Me.lvchAssemblies1.Text = "Key"
            Me.lvchAssemblies1.Width = 100
            '
            'lvchAssemblies2
            '
            Me.lvchAssemblies2.Text = "Value"
            Me.lvchAssemblies2.Width = 300
            '
            'cboAssemblies
            '
            Me.cboAssemblies.Dock = System.Windows.Forms.DockStyle.Top
            Me.cboAssemblies.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.cboAssemblies.Location = New System.Drawing.Point(0, 0)
            Me.cboAssemblies.Name = "cboAssemblies"
            Me.cboAssemblies.Size = New System.Drawing.Size(416, 20)
            Me.cboAssemblies.TabIndex = 0
            '
            'llHomePage
            '
            Me.llHomePage.Dock = System.Windows.Forms.DockStyle.Top
            Me.llHomePage.Location = New System.Drawing.Point(10, 10)
            Me.llHomePage.Name = "llHomePage"
            Me.llHomePage.Size = New System.Drawing.Size(424, 16)
            Me.llHomePage.TabIndex = 0
            Me.llHomePage.TabStop = True
            Me.llHomePage.Text = "Home Page"
            Me.llHomePage.TextAlign = System.Drawing.ContentAlignment.TopRight
            '
            'picLogo
            '
            Me.picLogo.Dock = System.Windows.Forms.DockStyle.Fill
            Me.picLogo.Image = CType(resources.GetObject("picLogo.Image"), System.Drawing.Image)
            Me.picLogo.Location = New System.Drawing.Point(10, 26)
            Me.picLogo.Name = "picLogo"
            Me.picLogo.Size = New System.Drawing.Size(424, 44)
            Me.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
            Me.picLogo.TabIndex = 1
            Me.picLogo.TabStop = False
            '
            'About
            '
            Me.AcceptButton = Me.btnOK
            Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
            Me.ClientSize = New System.Drawing.Size(444, 368)
            Me.Controls.Add(Me.Panel3)
            Me.Controls.Add(Me.Panel2)
            Me.Controls.Add(Me.Panel1)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "About"
            Me.ShowInTaskbar = False
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
            Me.Text = "About"
            Me.Panel1.ResumeLayout(False)
            Me.Panel2.ResumeLayout(False)
            Me.Panel3.ResumeLayout(False)
            Me.TabControl1.ResumeLayout(False)
            Me.TabPage1.ResumeLayout(False)
            Me.TabPage2.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub

#End Region

        Private Sub About_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

            With Me
                'Set the form's font to its owner's font, if it has a owner.
                If Not .Owner Is Nothing Then
                    .Font = .Owner.Font
                End If

                .Text = "About " & EntryAssembly.Title()  'Set the form's caption.


                'Populate application information.
                .lvApplication.Items.Clear()
                .AddListViewItem(Me.lvApplication, "Friendly Name", New String() {AppDomain.CurrentDomain().FriendlyName()})
                .AddListViewItem(Me.lvApplication, "Name", New String() {EntryAssembly.Name()})
                .AddListViewItem(Me.lvApplication, "Version", New String() {EntryAssembly.Version().ToString()})
                .AddListViewItem(Me.lvApplication, "Build Date", New String() {EntryAssembly.BuildDate().ToString()})
                .AddListViewItem(Me.lvApplication, "Location", New String() {EntryAssembly.Location()})
                .AddListViewItem(Me.lvApplication, "Title", New String() {EntryAssembly.Title()})
                .AddListViewItem(Me.lvApplication, "Description", New String() {EntryAssembly.Description()})
                .AddListViewItem(Me.lvApplication, "Company", New String() {EntryAssembly.Company()})
                .AddListViewItem(Me.lvApplication, "Product", New String() {EntryAssembly.Product()})
                .AddListViewItem(Me.lvApplication, "Copyright", New String() {EntryAssembly.Copyright()})
                .AddListViewItem(Me.lvApplication, "Trademark", New String() {EntryAssembly.Trademark()})



                'Populate the assembly ComboBox with the names of all the application assemblies.
                .cboAssemblies.Items.Clear()
                For Each oAssembly As Reflection.Assembly In AppDomain.CurrentDomain().GetAssemblies()
                    .cboAssemblies.Items.Add(oAssembly.GetName().Name())
                Next

                If .cboAssemblies.Items().Count() > 0 Then
                    .cboAssemblies.SelectedIndex = 0    'Select the first assembly name.
                End If
            End With

        End Sub

        Private Sub llHomePage_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles llHomePage.LinkClicked

            'Launch ESO web site in the default browser.
            Process.Start("http://opweb.cha.tva.gov")

        End Sub

        Private Sub cboAssemblies_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboAssemblies.SelectedIndexChanged

            Me.lvAssemblies.Items.Clear()   'Remove all previous entries from the ListView.


            Dim oAsmInfo As TVA.Shared.Assembly
            Try
                oAsmInfo = New TVA.Shared.Assembly(Me.GetAssembly(Me.cboAssemblies.SelectedItem()))

                'Display all the attributes exposed by the selected assembly.
                Dim nvcAssemblyAttributes As Specialized.NameValueCollection = oAsmInfo.GetAttributes()
                For Each strKey As String In nvcAssemblyAttributes
                    AddListViewItem(Me.lvAssemblies, strKey, New String() {nvcAssemblyAttributes(strKey)})
                Next
            Catch ex As Exception
                MsgBox("Cannot load assembly information.", MsgBoxStyle.Exclamation, "TVA.Forms.About")
            End Try

        End Sub

        Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click

            'Close the form.
            Me.Close()

        End Sub

        Private Function GetAssembly(ByVal AssemblyName As String) As Reflection.Assembly

            'Retrieve the assembly from the assembly name.
            For Each oAssembly As Reflection.Assembly In AppDomain.CurrentDomain().GetAssemblies()
                If oAssembly.GetName().Name().ToLower() = AssemblyName.ToLower() Then
                    Return oAssembly
                End If
            Next

        End Function

        Private Sub AddListViewItem(ByVal oListView As ListView, ByVal Text As String, ByVal Subitems As String())

            'Add a new ListViewItem with the specified data to the specified ListView.
            Dim lvi As New ListViewItem
            lvi.Text = Text
            For Each str As String In Subitems
                lvi.SubItems.Add(str)
            Next

            oListView.Items.Add(lvi)

        End Sub
    End Class

End Namespace
