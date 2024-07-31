# Generación y Configuración de Certificados para AFIP

## Pasos para generar la clave privada y la solicitud de certificado (CSR)

1. **Buscar y abrir OpenSSL:**
   - En tu computadora, busca "OpenSSL" y ejecuta la aplicación.

2. **Generar la clave privada:**
   ```sh
   openssl genrsa -out MiClavePrivada.key 2048

3. Generar la solicitud de certificado (CSR):
 
   ```sh
   openssl req -new -key MiClavePrivada.key -subj "/C=AR/O=Empresa Seba S.A./CN=mi certificado 1/serialNumber=CUIT 23365081999" -out MiPedidoCSR.csr

Estos comandos generarán dos archivos:

- MiClavePrivada.key (clave privada)
- MiPedidoCSR.csr (solicitud de certificado)

4. Obtener el certificado x509v2 desde AFIP:

- Accede al WSASS Autoservicio de Acceso a WebServices de la AFIP.
- Ir a "Nuevo Certificado" en el WSASS: WSASS AFIP (https://wsass-homo.afip.gob.ar/wsass/portal/Autoservicio/crearcomputador.aspx)
- Nombre simbólico del DN: Ingresa un alias para el computador fiscal (por ejemplo, PC1).
- Solicitud de certificado en formato PKCS#10: Abre MiPedidoCSR.csr con un editor de texto y copia su contenido aquí.
- Presiona "Crear DN y obtener certificado".
- Copia el contenido del campo "Resultado" en un archivo nuevo y guárdalo con la extensión .pem (este es el certificado x509v2).

5. Crear autorización a servicio:

- Ir a "Crear autorización a servicio" para asociar el certificado al webservice.
- Seleccionar el "Nombre simbólico del DN a autorizar" (igual que en el paso anterior).
- Revisar el CUIT representado y quien genera la autorización.
- Seleccionar el "Servicio al que desea acceder" (por ejemplo, "wsfe factura electrónica").
- Presionar "Crear autorización de Acceso" y revisar el resultado.

## Conversión del archivo .pem a .pfx
Convertir el archivo .pem a .pfx:

   ```sh
openssl pkcs12 -export -out certificado.pfx -inkey MiClavePrivada.key -in certificado.pem


Este comando pedirá un password para proteger el archivo .pfx.
