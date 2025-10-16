#!/bin/bash
# Script para borrar archivos antiguos en S3 y dejar solo los 10 más recientes

PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin

# Usar ruta relativa al directorio actual del script
SCRIPT_DIR=$(dirname "$0")
LOG_FILE="/home/ubuntu/borrar_antiguos_s3.log"

echo "Ejecutando borrado S3 a $(date) por usuario $(whoami)" >> "$LOG_FILE"

BUCKET="kilometrajesube"
PREFIX="procesados/"

# Verificar dependencias
if ! command -v jq >/dev/null 2>&1; then
    echo "Error: jq no está instalado. Instalando..." >> "$LOG_FILE"
    sudo apt update && sudo apt install jq -y >> "$LOG_FILE" 2>&1
    if ! command -v jq >/dev/null 2>&1; then
        echo "Error: No se pudo instalar jq" >> "$LOG_FILE"
        exit 1
    fi
fi

if ! command -v docker >/dev/null 2>&1; then
    echo "Error: docker no está instalado" >> "$LOG_FILE"
    exit 1
fi

# Función para ejecutar AWS CLI (local o Docker)
aws_cmd() {
    if command -v aws >/dev/null 2>&1; then
        aws "$@"
    else
        # Usar Docker si AWS CLI local no está disponible
        docker run --rm -v ~/.aws:/root/.aws -v $(pwd):/aws amazon/aws-cli "$@"
    fi
}

# Borrar archivos antiguos manteniendo solo los 10 más recientes
echo "Iniciando limpieza de archivos antiguos en S3..." >> "$LOG_FILE"
aws_cmd s3api list-objects-v2 --bucket "$BUCKET" --prefix "$PREFIX" --query "Contents[?ends_with(Key, '.csv')]" --output json 2>>"$LOG_FILE" | \
jq -r '.[] | "\(.LastModified) \(.Key)"' | \
sort | \
head -n -10 | \
awk '{print $2}' | \
while read key; do
    echo "Borrando $key" >> "$LOG_FILE"
    aws_cmd s3 rm "s3://$BUCKET/$key" >> "$LOG_FILE" 2>&1
    if [ $? -ne 0 ]; then
        echo "Error borrando $key a $(date)" >> "$LOG_FILE"
    fi
done

echo "Limpieza S3 completada a $(date)" >> "$LOG_FILE"
