#!/bin/bash
# Script para ejecutar el procesamiento de archivos CSV de kilometraje

cd /home/gavila/721/webApi || exit 1

dotnet run --process-csvs
