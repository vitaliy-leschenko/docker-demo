FROM microsoft/mssql-server-windows-express
EXPOSE 1433
VOLUME ["c:/data"]
RUN Set-ItemProperty -path 'HKLM:\software\microsoft\microsoft sql server\mssql14.SQLEXPRESS\mssqlserver\' -name DefaultData -value 'c:\\data'; \
    Set-ItemProperty -path 'HKLM:\software\microsoft\microsoft sql server\mssql14.SQLEXPRESS\mssqlserver\' -name DefaultLog -value 'c:\\data';