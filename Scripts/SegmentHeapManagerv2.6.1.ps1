<#
.SYNOPSIS
    Segment Heap Manager GUI

.DESCRIPTION
    - Toggles “Enable/Disable Segment Heap” via:
         HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Segment Heap\Enabled (DWORD 0 or 1)
    - Edits/Creates the four official heap parameters under:
         HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\
          • HeapSegmentReserve
          • HeapSegmentCommit
          • HeapDeCommitTotalFreeThreshold
          • HeapDeCommitFreeBlockThreshold
      When no override exists, the GUI will show the built-in default values.
    - Manages “Excluded Apps” (IFEO entries with FrontEndHeapDebugOptions=4).

    NOTE: Must run as Administrator. A reboot is required for any change to take effect.

.NOTES
    - Tested on Windows 10/11 with PowerShell ≥ 5.1.
    - If “SegmentHeap” subkey does not exist, it will be created automatically when you first Enable.
    - Fuck, Keep Reworking the whole thing gets me Tired.
    - Hope EVERYONE is Happy now.
    - Had no clue Moomy did gave me bad keys, lets hope the Keys from Questionable are valid.
#>


Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# ↪ Force Windows theming in the elevated process:
[System.Windows.Forms.Application]::EnableVisualStyles()
[System.Windows.Forms.Application]::SetCompatibleTextRenderingDefault($false)
#----------------------------------------------------------------------------------
# 1) Auto-elevate to Administrator if not already
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



Add-Type -AssemblyName System.Windows.Forms, System.Drawing

#----------------------------------------------------------------------------------
# 2) Registry Paths and Default Values
#----------------------------------------------------------------------------------
# Root of everything:
$SessionRoot = 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager'

# Subkey for Enable/Disable flag:
$EnableKey  = "$SessionRoot\Segment Heap"
$EnableName = 'Enabled'    # DWORD: 1 = Enabled, 0 = Disabled

# Four official heap-parameters (DWORD) and their built-in defaults:
$HeapParams = @{
    HeapSegmentReserve             = 0x00100000   # 1 MiB
    HeapSegmentCommit              = 0x00002000   # 8 KiB
    HeapDeCommitTotalFreeThreshold = 0x00010000   # 64 KiB
    HeapDeCommitFreeBlockThreshold = 0x00001000   # 4 KiB
}

# Descriptions (for the GUI):
$Descriptions = @{
    HeapSegmentReserve             = 'Bytes to reserve per LFH segment (1 MiB default).'
    HeapSegmentCommit              = 'Bytes to commit up-front in each LFH segment (8 KiB default).'
    HeapDeCommitTotalFreeThreshold = 'Total free-pages threshold (64 KiB) at which to decommit.'
    HeapDeCommitFreeBlockThreshold = 'Free-block size (4 KiB) that triggers a page decommit.'
}

# IFEO (excluded apps) roots:
$IFEO32 = 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options'
$IFEO64 = 'HKLM:\SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion\Image File Execution Options'

#----------------------------------------------------------------------------------
# 3) Helper Functions for Registry Operations
#----------------------------------------------------------------------------------

function Get-SegmentHeapStatus {
    <#
    .SYNOPSIS
        Returns “Enabled” if HKLM:\…\Segment Heap\Enabled = 1, else “Disabled” or “No Key.”
    #>
    if (-not (Test-Path $EnableKey)) {
        return 'Disabled (no <Segment Heap> key)'
    }
    $val = 0
    try {
        $val = (Get-ItemProperty -Path $EnableKey -Name $EnableName -ErrorAction SilentlyContinue).$EnableName
    } catch {
        $val = 0
    }
    if ($val -eq 1) { return 'Enabled' }
    elseif ($val -eq 0) { return 'Disabled' }
    else { return "Unknown (`$EnableName = $val)" }
}

function Set-SegmentHeapEnable {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory)][bool] $On
    )
    # Ensure the subkey exists
    if (-not (Test-Path $EnableKey)) {
        New-Item -Path $EnableKey -Force | Out-Null
    }
    $dwordValue = if ($On) { 1 } else { 0 }
    Set-ItemProperty -Path $EnableKey -Name $EnableName -Value ([uint32]$dwordValue) `
                     -Type DWord -Force -ErrorAction Stop
}

function Remove-HeapParameters {
    <#
    .SYNOPSIS
        Removes the four heap-parameter values from the Session Manager root.
        Does NOT touch <Segment Heap> subkey (Enabled flag).
    #>
    foreach ($name in $HeapParams.Keys) {
        if (Get-ItemProperty -Path $SessionRoot -Name $name -ErrorAction SilentlyContinue) {
            Remove-ItemProperty -Path $SessionRoot -Name $name -ErrorAction SilentlyContinue
        }
    }
}

function Get-Excluded {
    <#
    .SYNOPSIS
        Return a sorted list of executables that have IFEO\FrontEndHeapDebugOptions=4.
    #>
    $set = [Collections.Generic.HashSet[string]]::new()
    foreach ($base in $IFEO32, $IFEO64) {
        if (Test-Path $base) {
            Get-ChildItem -Path $base -ErrorAction SilentlyContinue | ForEach-Object {
                $childName = $_.PSChildName
                $frontOpt = (Get-ItemProperty -Path $_.PSPath -Name FrontEndHeapDebugOptions -ErrorAction SilentlyContinue).FrontEndHeapDebugOptions
                if ($frontOpt -eq 4) {
                    $set.Add($childName) | Out-Null
                }
            }
        }
    }
    return $set | Sort-Object
}

function Add-Excluded {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string] $exe
    )
    foreach ($base in $IFEO32, $IFEO64) {
        $path = Join-Path $base $exe
        if (-not (Test-Path $path)) {
            New-Item -Path $path -Force | Out-Null
        }
        Set-ItemProperty -Path $path -Name FrontEndHeapDebugOptions -Value 4 -Type DWord -Force -ErrorAction Stop
    }
}

function Remove-Excluded {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string] $exe
    )
    foreach ($base in $IFEO32, $IFEO64) {
        $path = Join-Path $base $exe
        if (Test-Path $path) {
            Remove-ItemProperty -Path $path -Name FrontEndHeapDebugOptions -ErrorAction SilentlyContinue
            # If nothing else remains in that IFEO\<exe> key, delete the key entirely
            $props = Get-ItemProperty -Path $path -ErrorAction SilentlyContinue
            if ($props.PSObject.Properties.Count -le 1) {
                Remove-Item -Path $path -Recurse -Force -ErrorAction SilentlyContinue
            }
        }
    }
}

# Convert between “0x…” hex or decimal strings → uint32
function Convert-HexOrDec {
    Param(
        [Parameter(Mandatory)][string] $InputValue
    )
    if ($InputValue -match '^0x[0-9A-Fa-f]+$') {
        return [Convert]::ToUInt32($InputValue,16)
    }
    elseif ([UInt32]::TryParse($InputValue,[ref]$null)) {
        return [UInt32] $InputValue
    }
    else {
        return $null
    }
}

#----------------------------------------------------------------------------------
# 4) Build the Windows Forms GUI
#----------------------------------------------------------------------------------

# Main form
$form = New-Object System.Windows.Forms.Form
$form.Text            = 'Segment Heap Manager'
$form.Size            = New-Object System.Drawing.Size(820,620)
$form.FormBorderStyle = 'FixedSingle'
$form.MaximizeBox     = $false
$form.StartPosition   = 'CenterScreen'

# TabControl
$tabs = New-Object System.Windows.Forms.TabControl
$tabs.Dock = 'Fill'
$form.Controls.Add($tabs)

# -------------------- OPTIONAL ICON --------------------
$iconBase64 = @"
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
"@
try {
    $iconBytes = [Convert]::FromBase64String($iconBase64)
    $ms = New-Object System.IO.MemoryStream(,$iconBytes)
    $bmp = [System.Drawing.Bitmap]::FromStream($ms)
    $icon = [System.Drawing.Icon]::FromHandle($bmp.GetHicon())
    $form.Icon = $icon
} catch {
    # Ignore if icon fails
}

# -------------------- STATUS TAB --------------------
$ts = New-Object System.Windows.Forms.TabPage('Status')

$lblStatus = New-Object System.Windows.Forms.Label
$lblStatus.Location = New-Object System.Drawing.Point(20,20)
$lblStatus.AutoSize = $true
$lblStatus.Font     = New-Object System.Drawing.Font('Segoe UI',10,[System.Drawing.FontStyle]::Bold)

$btnEnable  = New-Object System.Windows.Forms.Button
$btnEnable.Text     = 'Enable Segment Heap'
$btnEnable.Location = New-Object System.Drawing.Point(20,60)
$btnEnable.Size     = New-Object System.Drawing.Size(160,30)
$btnEnable.Add_Click({
    Set-SegmentHeapEnable $true
    [Windows.Forms.MessageBox]::Show(
        'Segment Heap has been enabled (Enabled=1). Restart required.',
        'Segment Heap Manager',
        [Windows.Forms.MessageBoxButtons]::OK,
        [Windows.Forms.MessageBoxIcon]::Information
    )
    RefreshUI
})

$btnDisable = New-Object System.Windows.Forms.Button
$btnDisable.Text     = 'Disable Segment Heap'
$btnDisable.Location = New-Object System.Drawing.Point(200,60)
$btnDisable.Size     = New-Object System.Drawing.Size(160,30)
$btnDisable.Add_Click({
    Set-SegmentHeapEnable $false
    [Windows.Forms.MessageBox]::Show(
        'Segment Heap has been disabled (Enabled=0). Restart required.',
        'Segment Heap Manager',
        [Windows.Forms.MessageBoxButtons]::OK,
        [Windows.Forms.MessageBoxIcon]::Information
    )
    RefreshUI
})

$btnRestart = New-Object System.Windows.Forms.Button
$btnRestart.Text     = 'Restart Now'
$btnRestart.Location = New-Object System.Drawing.Point(380,60)
$btnRestart.Size     = New-Object System.Drawing.Size(120,30)
$btnRestart.Add_Click({
    if ([Windows.Forms.MessageBox]::Show(
            'Are you sure you want to restart now?', 'Confirm Restart',
            [Windows.Forms.MessageBoxButtons]::YesNo,
            [Windows.Forms.MessageBoxIcon]::Warning
        ) -eq 'Yes')
    {
        Restart-Computer -Force
    }
})

$btnRefresh = New-Object System.Windows.Forms.Button
$btnRefresh.Text     = 'Refresh'
$btnRefresh.Location = New-Object System.Drawing.Point(520,60)
$btnRefresh.Size     = New-Object System.Drawing.Size(120,30)
$btnRefresh.Add_Click({
    RefreshUI
    [Windows.Forms.MessageBox]::Show('Refreshed.', 'Segment Heap Manager',
        [Windows.Forms.MessageBoxButtons]::OK,[Windows.Forms.MessageBoxIcon]::Information)
})

$txtInfo = New-Object System.Windows.Forms.TextBox
$txtInfo.Multiline   = $true
$txtInfo.ReadOnly    = $true
$txtInfo.ScrollBars  = 'Vertical'
$txtInfo.Location    = New-Object System.Drawing.Point(20,100)
$txtInfo.Size        = New-Object System.Drawing.Size(760,320)
$txtInfo.Font        = New-Object System.Drawing.Font('Microsoft Sans Serif',9)
$txtInfo.Text        = @"
Segment Heap Manager
— by Reiny Z.

Welcome to the Segment Heap Manager! This tool enables you to configure Windows Segment Heap settings and manage excluded applications directly through the registry.

Segment Heap can significantly enhance the performance of applications that perform frequent or intensive memory read/write operations. Unlike the default Low Fragmentation Heap (LFH), Segment Heap allows memory to be divided into multiple heaps, facilitating more efficient parallel access and reducing RAM contention.

By instructing Windows to use Segment Heap instead of LFH, you can unlock improved memory management and application responsiveness.

⚠ Note: A system restart is required for changes to take effect.

Use the tabs above to:
• Manage excluded applications
• Fine-tune heap parameters

Special thanks to the testers and friends from the Melodys and Alchemy IT Discord communities for their support and feedback.
"@

$ts.Controls.AddRange(@($lblStatus, $btnEnable, $btnDisable, $btnRestart, $btnRefresh, $txtInfo))
$tabs.TabPages.Add($ts)

# -------------------- EXCLUDED APPS TAB --------------------
$ta = New-Object System.Windows.Forms.TabPage('Excluded Apps')

$list = New-Object System.Windows.Forms.ListBox
$list.Location = New-Object System.Drawing.Point(20,20)
$list.Size     = New-Object System.Drawing.Size(350,420)

$txtApp = New-Object System.Windows.Forms.TextBox
$txtApp.Location = New-Object System.Drawing.Point(390,20)
$txtApp.Size     = New-Object System.Drawing.Size(300,25)

$btnAdd = New-Object System.Windows.Forms.Button
$btnAdd.Text     = 'Add'
$btnAdd.Location = New-Object System.Drawing.Point(390,60)
$btnAdd.Size     = New-Object System.Drawing.Size(100,30)
$btnAdd.Add_Click({
    try {
        Add-Excluded $txtApp.Text
        RefreshUI
    } catch {
        [Windows.Forms.MessageBox]::Show("Error: $_", 'Error Adding', 
            [Windows.Forms.MessageBoxButtons]::OK, [Windows.Forms.MessageBoxIcon]::Error)
    }
})

$btnRemove = New-Object System.Windows.Forms.Button
$btnRemove.Text     = 'Remove'
$btnRemove.Location = New-Object System.Drawing.Point(390,100)
$btnRemove.Size     = New-Object System.Drawing.Size(100,30)
$btnRemove.Add_Click({
    if ($list.SelectedItem) {
        Remove-Excluded $list.SelectedItem
        RefreshUI
    } else {
        [Windows.Forms.MessageBox]::Show('Please select an entry first.', 'Warning',
            [Windows.Forms.MessageBoxButtons]::OK, [Windows.Forms.MessageBoxIcon]::Warning)
    }
})

$ta.Controls.AddRange(@($list, $txtApp, $btnAdd, $btnRemove))
$tabs.TabPages.Add($ta)

# -------------------- HEAP PARAMETERS TAB --------------------
$tp = New-Object System.Windows.Forms.TabPage('Heap Parameters')

$dgv = New-Object System.Windows.Forms.DataGridView
$dgv.Location             = New-Object System.Drawing.Point(20,20)
$dgv.Size                 = New-Object System.Drawing.Size(780,450)
$dgv.AutoSizeColumnsMode  = 'Fill'
$dgv.AllowUserToAddRows    = $false

# Define columns: Parameter | Current (Hex) | Current (Dec) | Default (Hex) | Description
foreach ($col in @(
    @{Name='Parameter';   HeaderText='Parameter';    ReadOnly=$true},
    @{Name='CurrentHex';  HeaderText='Current (Hex)'; ReadOnly=$false},
    @{Name='CurrentDec';  HeaderText='Current (Dec)'; ReadOnly=$true},
    @{Name='DefaultHex';  HeaderText='Default (Hex)'; ReadOnly=$true},
    @{Name='Description'; HeaderText='Description';   ReadOnly=$true}
)) {
    $c = New-Object System.Windows.Forms.DataGridViewTextBoxColumn
    $c.Name       = $col.Name
    $c.HeaderText = $col.HeaderText
    $c.ReadOnly   = $col.ReadOnly
    $dgv.Columns.Add($c) | Out-Null
}
$tp.Controls.Add($dgv)

$btnSave = New-Object System.Windows.Forms.Button
$btnSave.Text     = 'Save Changes'
$btnSave.Location = New-Object System.Drawing.Point(20,490)
$btnSave.Size     = New-Object System.Drawing.Size(120,30)
$btnSave.Add_Click({
    $ok = $true
    foreach ($row in $dgv.Rows) {
        if ($row.IsNewRow) { continue }
        $param = $row.Cells['Parameter'].Value
        $hexStr = $row.Cells['CurrentHex'].Value
        $decStr = $row.Cells['CurrentDec'].Value

        # Convert user input (hex or decimal) into uint32
        $num = Convert-HexOrDec $hexStr
        if ($null -eq $num) {
            [UInt32]::TryParse($decStr,[ref]$num) | Out-Null
        }
        if ($null -eq $num) {
            $ok = $false; break
        }

        try {
            Set-ItemProperty -Path $SessionRoot -Name $param -Value ([uint32]$num) `
                             -Type DWord -ErrorAction Stop
        } catch {
            $ok = $false; break
        }
    }
    if ($ok) {
        [Windows.Forms.MessageBox]::Show(
            'Heap parameters saved. Restart required.',
            'Success', [Windows.Forms.MessageBoxButtons]::OK,
            [Windows.Forms.MessageBoxIcon]::Information
        )
    } else {
        [Windows.Forms.MessageBox]::Show(
            'Error saving parameters. Use “0x…” hex or valid decimal.',
            'Error', [Windows.Forms.MessageBoxButtons]::OK,
            [Windows.Forms.MessageBoxIcon]::Error
        )
    }
    RefreshUI
})
$tp.Controls.Add($btnSave)

$btnReload = New-Object System.Windows.Forms.Button
$btnReload.Text     = 'Reload'
$btnReload.Location = New-Object System.Drawing.Point(160,490)
$btnReload.Size     = New-Object System.Drawing.Size(120,30)
$btnReload.Add_Click({ RefreshUI })
$tp.Controls.Add($btnReload)

$tabs.TabPages.Add($tp)

# -------------------- REFRESH UI FUNCTION --------------------
function RefreshUI {
    # 1) Status label
    $s = Get-SegmentHeapStatus
    $lblStatus.Text = "Segment Heap Status: $s"
    if ($s -eq 'Enabled') {
        $lblStatus.ForeColor = [Drawing.Color]::Green
    } else {
        $lblStatus.ForeColor = [Drawing.Color]::Red
    }

    # 2) Excluded Apps list
    $list.Items.Clear()
    Get-Excluded | ForEach-Object { $list.Items.Add($_) }

    # 3) Heap Parameters grid
    $dgv.Rows.Clear()
    foreach ($k in $HeapParams.Keys) {
        # If a registry override exists, use it; otherwise, use the built-in default.
        try {
            $raw = (Get-ItemProperty -Path $SessionRoot -Name $k -ErrorAction SilentlyContinue).$k
        } catch {
            $raw = $null
        }
        if ($null -eq $raw) {
            # No registry value → show default
            $display = $HeapParams[$k]
        } else {
            $display = $raw
        }

        $hexCur = '0x{0:X}' -f $display
        $decCur = $display.ToString()
        $hexDef = '0x{0:X}' -f $HeapParams[$k]
        $desc   = $Descriptions[$k]
        $dgv.Rows.Add($k, $hexCur, $decCur, $hexDef, $desc)
    }
}


# Launch the GUI
# Fuck me, Daddy!
$form.Add_Shown({ RefreshUI })
[void] $form.ShowDialog()
