# Diagnóstico final: arranca la app, recorre todas las rutas y la detiene.
$ErrorActionPreference = 'Continue'

$raiz = 'c:\Users\aldor\Programa4'
$archivo = Join-Path $raiz 'diag-rutas.txt'
Set-Location (Join-Path $raiz 'SistemaUsuarios')

function Escribir {
    param([string]$Linea)
    Add-Content -Path $archivo -Value $Linea -Encoding utf8
}

Remove-Item -Force $archivo -ErrorAction SilentlyContinue
Escribir 'DIAGNOSTICO FINAL DE RUTAS'

Get-Process 'SistemaUsuarios' -ErrorAction SilentlyContinue | ForEach-Object {
    Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
}

Get-NetTCPConnection -LocalPort 5110 -State Listen -ErrorAction SilentlyContinue | ForEach-Object {
    Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
}

$proceso = Start-Process -FilePath 'dotnet' `
    -ArgumentList @('run', '--no-build', '--urls', 'http://localhost:5110') `
    -PassThru `
    -RedirectStandardOutput (Join-Path $raiz 'fix-app-out.log') `
    -RedirectStandardError (Join-Path $raiz 'fix-app-err.log')

Start-Sleep -Seconds 18

function Invoke-Pagina {
    param([string]$Ruta)

    try {
        $respuesta = Invoke-WebRequest -Uri ('http://localhost:5110' + $Ruta) -UseBasicParsing -TimeoutSec 45
        return @{ Codigo = [int]$respuesta.StatusCode; Contenido = [string]$respuesta.Content }
    }
    catch {
        $respuesta = $_.Exception.Response

        if ($null -ne $respuesta) {
            $lector = New-Object System.IO.StreamReader($respuesta.GetResponseStream())
            return @{ Codigo = [int]$respuesta.StatusCode; Contenido = $lector.ReadToEnd() }
        }

        return @{ Codigo = -1; Contenido = $_.Exception.Message }
    }
}

$rutas = @(
    '/',
    '/usuarios',
    '/usuarios/nuevo',
    '/usuarios/editar/1',
    '/usuarios/editar/999',
    '/tipos-usuario',
    '/tipos-usuario/nuevo',
    '/tipos-usuario/editar/1',
    '/tipos-usuario/editar/999'
)

foreach ($ruta in $rutas) {
    $pagina = Invoke-Pagina -Ruta $ruta
    $detalle = if ($pagina.Codigo -eq 200) { 'OK' } else { 'REVISAR' }
    Escribir "GET  $ruta -> HTTP $($pagina.Codigo) ($detalle, $($pagina.Contenido.Length) bytes)"
}

Escribir 'FIN DEL DIAGNOSTICO'

if ($proceso -and -not $proceso.HasExited) {
    Stop-Process -Id $proceso.Id -Force -ErrorAction SilentlyContinue
}

Start-Sleep -Seconds 2

Get-Process 'SistemaUsuarios' -ErrorAction SilentlyContinue | ForEach-Object {
    Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
}

Get-NetTCPConnection -LocalPort 5110 -State Listen -ErrorAction SilentlyContinue | ForEach-Object {
    Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
}
