# Elevate if needed (PowerShell 5.1+ and 7+ compatible, no console window)
if (-not ([Security.Principal.WindowsPrincipal] `
    [Security.Principal.WindowsIdentity]::GetCurrent()
    ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {

    $exe = if ($PSVersionTable.PSEdition -eq 'Core') { "pwsh.exe" } else { "powershell.exe" }

    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $exe
    $psi.Arguments = "-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File `"$PSCommandPath`""
    $psi.Verb = "runas"
    $psi.UseShellExecute = $true
    $psi.CreateNoWindow = $true

    try {
        [System.Diagnostics.Process]::Start($psi) | Out-Null
    } catch {
        Add-Type -AssemblyName System.Windows.Forms
        [System.Windows.Forms.MessageBox]::Show("Elevation required. Script will now exit.", "Access Denied", "OK", "Error")
    }

    exit
}

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# ↪ Force Windows theming in the elevated process:
[System.Windows.Forms.Application]::EnableVisualStyles()
[System.Windows.Forms.Application]::SetCompatibleTextRenderingDefault($false)

# Registry base path for IFEO segment heap flag
$segmentHeapKeyPath = 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options'

function Get-EnabledApps {
    # Return only those subkeys where FrontEndHeapDebugOptions = 0x08
    Get-ChildItem $segmentHeapKeyPath -ErrorAction SilentlyContinue |
        Where-Object {
            $val = (Get-ItemProperty $_.PSPath -Name 'FrontEndHeapDebugOptions' -ErrorAction SilentlyContinue).FrontEndHeapDebugOptions
            ($val -eq 0x08)
        } |
        Select-Object -ExpandProperty PSChildName
}

function Refresh-ListView {
    $listView.Items.Clear()
    Get-EnabledApps | ForEach-Object {
        $item = New-Object System.Windows.Forms.ListViewItem($_)
        $listView.Items.Add($item) | Out-Null
    }
}

function Enable-SegmentHeap ($exeName) {
    if (-not $exeName.EndsWith(".exe", [System.StringComparison]::InvariantCultureIgnoreCase)) {
        $exeName += ".exe"
    }
    $regPath = Join-Path $segmentHeapKeyPath $exeName
    # Create or open the key, then set FrontEndHeapDebugOptions to 0x08
    New-Item -Path $regPath -Force | Out-Null
    Set-ItemProperty -Path $regPath -Name "FrontEndHeapDebugOptions" -Value 0x08 -Type DWord
    Refresh-ListView
}

function Disable-SegmentHeap ($exeName) {
    if (-not $exeName.EndsWith(".exe", [System.StringComparison]::InvariantCultureIgnoreCase)) {
        $exeName += ".exe"
    }
    $regPath = Join-Path $segmentHeapKeyPath $exeName
    if (Test-Path $regPath) {
        # Change value to 0x04 (legacy NT Heap). If the value doesn't exist, show info.
        $current = (Get-ItemProperty $regPath -Name "FrontEndHeapDebugOptions" -ErrorAction SilentlyContinue).FrontEndHeapDebugOptions
        if ($null -ne $current) {
            Set-ItemProperty -Path $regPath -Name "FrontEndHeapDebugOptions" -Value 0x04 -Type DWord
            Refresh-ListView
        } else {
            [System.Windows.Forms.MessageBox]::Show(
                "No Segment Heap setting was found to disable.",
                "Nothing to Disable",
                [System.Windows.Forms.MessageBoxButtons]::OK,
                [System.Windows.Forms.MessageBoxIcon]::Information
            )
        }
    } else {
        [System.Windows.Forms.MessageBox]::Show(
            "That executable isn't in the registry.",
            "Nothing to Disable",
            [System.Windows.Forms.MessageBoxButtons]::OK,
            [System.Windows.Forms.MessageBoxIcon]::Information
        )
    }
}

function Remove-SegmentHeap ($exeName) {
    if (-not $exeName.EndsWith(".exe", [System.StringComparison]::InvariantCultureIgnoreCase)) {
        $exeName += ".exe"
    }
    $regPath = Join-Path $segmentHeapKeyPath $exeName
    if (Test-Path $regPath) {
        $current = (Get-ItemProperty $regPath -Name "FrontEndHeapDebugOptions" -ErrorAction SilentlyContinue).FrontEndHeapDebugOptions
        if ($null -ne $current) {
            Remove-ItemProperty -Path $regPath -Name "FrontEndHeapDebugOptions" -ErrorAction SilentlyContinue
            Refresh-ListView
        } else {
            [System.Windows.Forms.MessageBox]::Show(
                "There's no Segment Heap flag to remove for that executable.",
                "Nothing to Remove",
                [System.Windows.Forms.MessageBoxButtons]::OK,
                [System.Windows.Forms.MessageBoxIcon]::Information
            )
        }
    } else {
        [System.Windows.Forms.MessageBox]::Show(
            "That executable isn't in the registry.",
            "Nothing to Remove",
            [System.Windows.Forms.MessageBoxButtons]::OK,
            [System.Windows.Forms.MessageBoxIcon]::Information
        )
    }
}

# ---------- GUI Setup ----------

$form = New-Object System.Windows.Forms.Form
$form.Text = "Segment Heap Nano Enabler"
$form.Size = New-Object System.Drawing.Size(580, 420)
$form.StartPosition = "CenterScreen"
$form.FormBorderStyle = [System.Windows.Forms.FormBorderStyle]::FixedDialog
$form.MaximizeBox = $false

$listView = New-Object System.Windows.Forms.ListView
$listView.View = 'Details'
$listView.FullRowSelect = $true
$listView.Columns.Add("Enabled Executables", 260)
$listView.Location = New-Object System.Drawing.Point(10, 10)
$listView.Size = New-Object System.Drawing.Size(280, 360)
$form.Controls.Add($listView)

$textBox = New-Object System.Windows.Forms.TextBox
$textBox.Location = New-Object System.Drawing.Point(310, 20)
$textBox.Size = New-Object System.Drawing.Size(180, 20)
$form.Controls.Add($textBox)

$browseButton = New-Object System.Windows.Forms.Button
$browseButton.Text = "Browse..."
$browseButton.Location = New-Object System.Drawing.Point(500, 18)
$browseButton.Size = New-Object System.Drawing.Size(70, 24)
$form.Controls.Add($browseButton)

$enableButton = New-Object System.Windows.Forms.Button
$enableButton.Text = "Enable Segment Heap"
$enableButton.Location = New-Object System.Drawing.Point(310, 60)
$enableButton.Size = New-Object System.Drawing.Size(260, 30)
$form.Controls.Add($enableButton)

$disableButton = New-Object System.Windows.Forms.Button
$disableButton.Text = "Disable Segment Heap"
$disableButton.Location = New-Object System.Drawing.Point(310, 100)
$disableButton.Size = New-Object System.Drawing.Size(260, 30)
$form.Controls.Add($disableButton)

$removeButton = New-Object System.Windows.Forms.Button
$removeButton.Text = "Remove Segment Heap Flag"
$removeButton.Location = New-Object System.Drawing.Point(310, 140)
$removeButton.Size = New-Object System.Drawing.Size(260, 30)
$form.Controls.Add($removeButton)

# File Dialog
$openFileDialog = New-Object System.Windows.Forms.OpenFileDialog
$openFileDialog.Filter = "Executable Files (*.exe)|*.exe"

# ---------- Event Handlers ----------

# Browse button: populate textbox with chosen .exe filename
$browseButton.Add_Click({
    if ($openFileDialog.ShowDialog() -eq [System.Windows.Forms.DialogResult]::OK) {
        $textBox.Text = [System.IO.Path]::GetFileName($openFileDialog.FileName)
    }
})

# Enable button: set FrontEndHeapDebugOptions = 0x08
$enableButton.Add_Click({
    $inputName = $textBox.Text.Trim()
    if (-not [string]::IsNullOrWhiteSpace($inputName)) {
        Enable-SegmentHeap $inputName
    } else {
        [System.Windows.Forms.MessageBox]::Show(
            "Please enter or browse for an executable to enable.",
            "Input Required",
            [System.Windows.Forms.MessageBoxButtons]::OK,
            [System.Windows.Forms.MessageBoxIcon]::Information
        )
    }
})

# Disable button: set FrontEndHeapDebugOptions = 0x04 if it exists, with match-checking
$disableButton.Add_Click({
    $selectedItems = $listView.SelectedItems
    $inputName = $textBox.Text.Trim()
    if ($selectedItems.Count -gt 0) {
        $selectedName = $selectedItems[0].Text
        if ($inputName -ieq $selectedName) {
            Disable-SegmentHeap $selectedName
        } else {
            [System.Windows.Forms.MessageBox]::Show(
                "Hmm… that’s not the same name! Jedi mind trick rejected.",
                "Mismatch!",
                [System.Windows.Forms.MessageBoxButtons]::OK,
                [System.Windows.Forms.MessageBoxIcon]::Warning
            )
        }
    } elseif ($inputName) {
        Disable-SegmentHeap $inputName
    } else {
        [System.Windows.Forms.MessageBox]::Show(
            "Select an item or enter a name to disable.",
            "No Selection",
            [System.Windows.Forms.MessageBoxButtons]::OK,
            [System.Windows.Forms.MessageBoxIcon]::Information
        )
    }
})

# Remove button: delete only the FrontEndHeapDebugOptions value, with match-checking
$removeButton.Add_Click({
    $selectedItems = $listView.SelectedItems
    $inputName = $textBox.Text.Trim()
    if ($selectedItems.Count -gt 0) {
        $selectedName = $selectedItems[0].Text
        if ($inputName -ieq $selectedName) {
            Remove-SegmentHeap $selectedName
        } else {
            [System.Windows.Forms.MessageBox]::Show(
                "Whoa there! The names don't match, so I won't remove anything.",
                "Name Mismatch",
                [System.Windows.Forms.MessageBoxButtons]::OK,
                [System.Windows.Forms.MessageBoxIcon]::Warning
            )
        }
    } elseif ($inputName) {
        Remove-SegmentHeap $inputName
    } else {
        [System.Windows.Forms.MessageBox]::Show(
            "Select an item or enter a name to remove.",
            "No Selection",
            [System.Windows.Forms.MessageBoxButtons]::OK,
            [System.Windows.Forms.MessageBoxIcon]::Information
        )
    }
})

# Initialize list on startup
Refresh-ListView

# Show the form
$form.Add_Shown({ $form.Activate() })
[void]$form.ShowDialog()
