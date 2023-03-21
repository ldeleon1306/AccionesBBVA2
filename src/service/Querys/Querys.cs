using System;
using System.Collections.Generic;
using System.Text;

namespace AccionesBBVA
{
    public class Querys
    {
        public string query(string fechaF,string fechaT,string tabla)
        {
            string query = "SELECT SUBSTRING(er.Registro,3,21) + SUBSTRING(er.Registro,85,3) + SUBSTRING(er.Registro,94,180)" +
                          "FROM AccionesBBVA.."+tabla+" (nolock) ER " +
                          "where ER.codigoAccion IN (" +
                          "'003'," +
                          "'006'," +
                          "'016'" +
                          ")" +
                          "and er.fechaCreacion Between '"+ fechaF + "' and '" + fechaT + "' " +
                          "and ER.Respuesta IN (" +
                          "4" +
                          ")" +
                          "and" +
                          "(" +
                          "er.observaciones like 'El estado del envio no permite realizar la operación. Envío en estado final. Estado: 6' " +
                          "OR  er.observaciones like 'El estado del envio no permite realizar la operación. Envío en estado final. Estado: 7' " +
                          "OR  er.observaciones like 'El estado del envio no permite realizar la operación. Envío en estado final. Estado: 8'" +
                          ") " +
                          "AND not exists (" +
                          "select 1 from AccionesBBVA.." + tabla + " (nolock) ER2 " +
                          "where " +
                          "er.NumeroInterno = er2.NumeroInterno " +
                          "and er2.respuesta <> 4 " +
                          "and er.codigoAccion = er2.codigoAccion " +
                          "and er.fechaCreacion < er2.fechaCreacion " +
                          "and er.id <> er2.id" +
                          ")";

            return query;
        }
        
    }
}
