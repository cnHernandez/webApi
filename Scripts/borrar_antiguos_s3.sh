#!/bin/bash
# Script para borrar archivos antiguos en S3 y dejar solo los 10 más recientes

BUCKET="kilometrajesube"
PREFIX="procesados/"
# Requiere AWS CLI configurado con permisos

aws s3api list-objects-v2 --bucket "$BUCKET" --prefix "$PREFIX" --query "Contents[?ends_with(Key, '.csv')]" --output json | \
jq -r '.[] | "\(.LastModified) \(.Key)"' | \
sort | \
head -n -10 | \
awk '{print $2}' | \
while read key; do
    echo "Borrando $key"
    aws s3 rm "s3://$BUCKET/$key"
done
