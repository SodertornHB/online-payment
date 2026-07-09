<#
.SYNOPSIS
    Skyddar bas-repots (GitHub) filer fran att skrivas over av organizational-specific
    vid build och av misstag committas.

.DESCRIPTION
    Byggets MSBuild-target `CopyOrgSpecificFiles` kopierar org-specifika filer fran
    ..\organizational-specific\ IN i det sparade kalltradet (OnlinePayment.Web\...).
    Dessa org-versioner (med t.ex. [NoLibraryAuth] och Sh.Library-referenser) hor INTE
    hemma i det publika bas-repot. Efter ett bygge visar `git status` dem som andrade,
    och ett `git commit -a` sveper in dem -> lackage till GitHub. Sa har det regredierat
    tidigare (jfr commit 28eb1d3, okt 2024).

    Det har skriptet satter `git update-index --skip-worktree` pa de berorda filerna.
    Git ignorerar da lokala andringar i dem: bas-versionen forblir det som ar spart,
    build-kopiorna gar inte att committa av misstag, och SH:s drift paverkas inte
    (bygget anvander anda den inkopierade org-versionen pa disk).

    KOR skriptet nar filerna ar i bas-tillstand (dvs. rent arbetstrad, direkt efter
    en `git checkout`/pull och FORE ett bygge).

.PARAMETER Unprotect
    Tar bort skyddet (--no-skip-worktree). Anvand nar du medvetet vill uppdatera
    BAS-versionen av en av filerna: kor -Unprotect, redigera, committa, kor sedan
    skriptet igen for att aterstalla skyddet.

.NOTES
    BEGRANSNING: skip-worktree-biten ar lokal per klon och foljer inte med i repot.
    Skriptet maste koras en gang per klon/byggmaskin. For en repo-delad, mer robust
    variant kan en pre-commit-hook anvandas i stallet (se README/diskussion).
#>
[CmdletBinding()]
param(
    [switch]$Unprotect
)

$ErrorActionPreference = 'Stop'

# Filer vars bas-version (GitHub) skrivs over av organizational-specific vid build.
# Sokvagar relativt repo-roten.
$protectedFiles = @(
    'OnlinePayment.Web/ApiController/PaymentApiControllerExtended.cs',
    'OnlinePayment.Web/Controller/HomeControllerExtended.cs',
    'OnlinePayment.Web/Controller/PaymentCallbackControllerExtended.cs',
    'OnlinePayment.Web/Controller/PaymentControllerExtended.cs',
    'OnlinePayment.Web/StartupExtended.cs',
    'OnlinePayment.Web/Web.csproj'
)

# Kor alltid fran repo-roten (skriptet ligger i scripts\).
$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

$flag = if ($Unprotect) { '--no-skip-worktree' } else { '--skip-worktree' }
$verb = if ($Unprotect) { 'Tar bort skydd fran' } else { 'Skyddar' }

foreach ($file in $protectedFiles) {
    if (-not (Test-Path $file)) {
        Write-Warning "Hoppar over (saknas): $file"
        continue
    }
    git update-index $flag -- $file
    Write-Host "$verb $file"
}

Write-Host ''
Write-Host 'Klart. Filer med skip-worktree-biten satt just nu:'
git ls-files -v | Where-Object { $_ -cmatch '^S ' } | ForEach-Object { "  $_" }
