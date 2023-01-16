using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Back_End.Clases
{
    public class ConexionBD
    {
        // Campos
        //variable para saber si el error que se produjo al intentar conectarse al servidor fue por conexión rota
        //para que intente reconectarse nuevamente.
        //private const String MENSAJER_ERROR_CONEXION = "A transport-level error has occurred when sending the request to the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)";
        private string servidor;
        private string baseDatos;

        private readonly AppSettings AppSettings = new AppSettings();

        #region PropiedadesConfiguracion
        // Propiedades de Configuracion
        private string CadenaConexion
        {
            get
            {
                return ObtenerCadenaConexion();
            }
        }
        public Controllers.ControladorBase Controlador { get; set; }
        #endregion


        #region Propiedades SQL
        // Propiedades SQL
        /// <summary>
        /// Indica si la hay una transacción abierta
        /// </summary>
        public bool TransaccionAbierta
        {
            get
            {
                return this.Transaccion != null;
            }
        }
        private SqlTransaction Transaccion { get; set; }
        private SqlConnection Conexion { get; set; }
        private bool ConexionCreada
        {
            get
            {
                return this.Conexion != null;
            }
        }
        /// <summary>
        /// Tipo de espera para la ejecucion de un <see cref="SqlCommand"/>
        /// </summary>
        public int TiempoEsperaSegundos { get; set; } = 30;
        /// <summary>
        /// Obtiene el nombre del servidor de la cadena de conexión.
        /// </summary>
        public string Servidor
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.servidor))
                {
                    string cadenaCnx;
                    string[] valores;

                    cadenaCnx = this.CadenaConexion;
                    valores = cadenaCnx.Split(';');

                    this.baseDatos = valores.Last(c => c.Contains("Initial Catalog="));
                    this.servidor = valores.Last(c => c.Contains("Data Source="));

                    this.baseDatos = this.baseDatos.Replace("Initial Catalog=", "").Replace(";", "");
                    this.servidor = this.servidor.Replace("Data Source=", "").Replace(";", "");
                }

                return this.servidor;
            }
        }
        /// <summary>
        /// Obtiene la base de datos de la cadena de conexión.
        /// </summary>
        public string BaseDeDatos
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.baseDatos))
                {
                    string cadenaCnx;
                    string[] valores;

                    cadenaCnx = this.CadenaConexion;
                    valores = cadenaCnx.Split(';');

                    this.baseDatos = valores.Last(c => c.Contains("Initial Catalog="));
                    this.servidor = valores.Last(c => c.Contains("Data Source="));

                    this.baseDatos = this.baseDatos.Replace("Initial Catalog=", "").Replace(";", "");
                    this.servidor = this.servidor.Replace("Data Source=", "").Replace(";", "");
                }

                return this.baseDatos;
            }
        }
        #endregion


        #region Constructores
        /// <summary>
        /// Inicializa la conexion a la Base de Datos configurandola segun la empresa indicada.
        /// </summary>
        public ConexionBD(Controllers.ControladorBase controlador)
        {
            this.Controlador = controlador;
        }
        #endregion


        #region Funciones

        #region Abrir y cerrar conexion
        private string ObtenerCadenaConexion()
        {
            return this.AppSettings.Conexion; //ConfigurationManager.ConnectionStrings["BD_JAPAC"].ConnectionString;
        }
        // Abrir y cerrar conexion
        private bool CrearConexion()
        {
            if (!this.ConexionCreada)
            {
                this.Conexion = new SqlConnection(CadenaConexion);
            }

            return this.ProbarConexion();
        }
        private void AbrirConexion()
        {
            if (this.ConexionCreada)
            {
                if (this.Conexion.State != ConnectionState.Open)
                {
                    this.Conexion.Open();
                }
            }
        }
        private void CerrarConexion()
        {
            if (this.Conexion.State == ConnectionState.Open)
            {
                this.Conexion.Close();
            }

            if (this.Conexion != null)
            {
                this.Conexion.Dispose();
                this.Conexion = null;
            }
        }
        private bool ProbarConexion()
        {
            try
            {
                if (this.ConexionCreada)
                {
                    if (this.Conexion.State != ConnectionState.Open)
                    {
                        this.Conexion.Open();
                        this.Conexion.Close();
                    }

                    return true;
                }
            }
            catch (SqlException)
            {
                // No hace nada
            }

            return false;
        }
        // Termina Abrir y cerrar conexion
        //private bool ErrorConexionRota(string mensaje)
        //{
        //    return MENSAJER_ERROR_CONEXION.ToUpper() == mensaje.Trim().ToUpper();
        //}
        #endregion


        #region Abrir, Cerrar y deshacer transaccion
        // Abrir, cerrar y deshacer transaccion
        /// <summary>
        /// Inicia una transaccion
        /// </summary>
        public void AbrirTransaccion()
        {
            if (!this.TransaccionAbierta)
            {
                if (this.ConexionCreada)
                {
                    if (this.Conexion.State != ConnectionState.Open)
                    {
                        this.Conexion.Open();
                    }
                }
                else
                {
                    this.CrearConexion();
                    this.Conexion.Open();
                }

                this.Transaccion = this.Conexion.BeginTransaction();
            }
        }
        /// <summary>
        /// Cierra la transaccion abierta
        /// </summary>
        public void CerrarTransaccion()
        {
            if (this.TransaccionAbierta)
            {
                this.Transaccion.Commit();
                this.Transaccion.Dispose();
                this.Transaccion = null;

                this.CerrarConexion();
            }
        }
        /// <summary>
        /// Realiza un Rollback en la transaccion abierta
        /// </summary>
        public void DeshacerTransaccion()
        {
            if (this.TransaccionAbierta)
            {
                this.Transaccion.Rollback();
                this.Transaccion.Dispose();
                this.Transaccion = null;

                CerrarConexion();
            }
        }
        // Termina Abrir, cerrar y deshacer transaccion
        #endregion


        #region Ejecucion de instrucciones SQL
        /// <summary>
        /// Crea una parametro con los valors ingresados
        /// </summary>
        /// <param name="nombre">Nombre del parametro, no incluir @</param>
        /// <param name="valor">Valor del parametro</param>
        /// <returns>Retorna un <see cref="ParametroSql"/> con los valores ingresados.</returns>
        public ParametroSql CrearParametro(string nombre, object valor) => this.CrearParametro(nombre, valor, false);
        /// <summary>
        /// Crea un parametro con los valores ingresados
        /// </summary>
        /// <param name="nombre">Nombre del parametro, no incluir @</param>
        /// <param name="valor">Valor del parametro</param>
        /// <param name="inOutPut">Inidca si sera de entrada y salida (InOutPut), <code>true</code> para InOutPut y <code>false</code> para Input</param>
        /// <returns>Retorna un <see cref="ParametroSql"/> con los valores ingresados.</returns>
        public ParametroSql CrearParametro(string nombre, object valor, bool inOutPut) => new ParametroSql(nombre, valor, inOutPut);
        // Ejecucion de comandos
        /// <summary>
        /// Crea un <code>SqlCommand</code> con el procedimiento y parametros indicados, este parametro tiene configurada la conexión ni la transacción.
        /// </summary>
        /// <param name="nombreProcedimiento">Nombre del procedimiento</param>
        /// <param name="parametros">Parametros del procedimiento, no incluir la @ en el nombre</param>
        /// <returns>Retorna un <code>SqlCommand</code></returns>
        private SqlCommand CrearComandoSql(string nombreProcedimiento, params ParametroSql[] parametros)
        {
            SqlCommand sqlCom = new SqlCommand(nombreProcedimiento);

            try
            {
                sqlCom.CommandType = CommandType.StoredProcedure;

                foreach (ParametroSql p in parametros)
                {
                    SqlParameter sqlParm = sqlCom.Parameters.AddWithValue(p.Nombre, p.Valor);

                    if (p.InOutPut)
                    {
                        sqlParm.Direction = ParameterDirection.InputOutput;
                    }
                }

                sqlCom.CommandTimeout = this.TiempoEsperaSegundos;
            }
            catch
            {
                throw;
            }

            return sqlCom;
        }


        /// <summary>
        /// Ejecuta un procedimiento almacenado de consulta y obtiene el valor de la primer fila y primer columna.
        /// </summary>
        /// <typeparam name="T">Tipo de datos que retornara</typeparam>
        /// <param name="nombreProcedimiento">Nombre del procedimiento almacenado</param>
        /// <param name="valorDefault">Valor por default en caso de no encontrar datos</param>
        /// <param name="parametros">Parametros del procedimiento, no incluir la @ en el nombre</param>
        /// <returns></returns>
        public async Task<T> RegresaDatoSQL<T>(string nombreProcedimiento, T valorDefault, params ParametroSql[] parametros)
        {
            bool cerrarConexion = false;
            SqlCommand sqlCom = null;

            try
            {
                object valor;

                if (!this.ConexionCreada)
                {
                    cerrarConexion = true;
                    if (!this.CrearConexion()) { return valorDefault; }
                }

                // Internamente la funcion valida si la conexion ya esta creada de ser asi no crea una nueva y valida si puede conectarse a la BD.
                if (!this.CrearConexion()) { return valorDefault; }

                sqlCom = this.CrearComandoSql(nombreProcedimiento, parametros);
                sqlCom.Connection = this.Conexion;
                sqlCom.Transaction = this.Transaccion; // Si no hay transacción es null

                AbrirConexion();

                valor = await sqlCom.ExecuteScalarAsync();

                if (DBNull.Value == valor || valor == null) { valor = valorDefault; }

                return (T)valor;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cerrarConexion) { this.CerrarConexion(); }
                Utilerias.DestruirSqlCommand(ref sqlCom);
            }
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado con mas de una consulta y retorna un DataSet de forma asincronica
        /// </summary>
        /// <param name="nombreProcedimiento">Nombre del procedimiento almacenado</param>
        /// <param name="parametros">Parametros del procedimiento, no debe incluir @ en su nombre</param>
        /// <returns></returns>
        public async Task<DataSet> EjecutaConsulta(string nombreProcedimiento, params ParametroSql[] parametros)
        {
            bool cerrarConexion = false;
            SqlDataAdapter da = null;
            DataSet ds = null;
            SqlCommand sqlCom = null;

            try
            {
                if (!this.ConexionCreada)
                {
                    cerrarConexion = true;
                    if (!this.CrearConexion()) { return null; }
                }

                sqlCom = this.CrearComandoSql(nombreProcedimiento, parametros);
                sqlCom.Connection = this.Conexion;
                sqlCom.Transaction = this.Transaccion; // Si no hay transaccion es null, entonces no hay necesidad de validar si hay transaccion

                da = new SqlDataAdapter(sqlCom);
                ds = new DataSet();

                await Task.Run(() => da.Fill(ds));

                return ds;
            }
            catch
            {
                throw;
            }
            finally
            {

                Utilerias.DestruirDataAdapter(ref da);
                Utilerias.DestruirDataSet(ref ds);
                Utilerias.DestruirSqlCommand(ref sqlCom);
                if (cerrarConexion) { this.CerrarConexion(); }
            }
        }
        /// <summary>
        /// Ejecuta un procedimiento almacenado con mas de una consulta y retorna un DataSet de forma sincronica
        /// </summary>
        /// <param name="nombreProcedimiento">Nombre del procedimiento almacenado</param>
        /// <param name="parametros">Parametros del procedimiento, no debe incluir @ en su nombre</param>
        /// <returns></returns>
        public DataSet EjecutaConsultaSync(string nombreProcedimiento, params ParametroSql[] parametros)
        {
            bool cerrarConexion = false;
            SqlDataAdapter da = null;
            DataSet ds;
            SqlCommand sqlCom = null;

            try
            {
                if (!this.ConexionCreada)
                {
                    cerrarConexion = true;
                    if (!this.CrearConexion()) { return null; }
                }

                sqlCom = this.CrearComandoSql(nombreProcedimiento, parametros);
                sqlCom.Connection = this.Conexion;
                sqlCom.Transaction = this.Transaccion; // Si no hay transaccion es null, entonces no hay necesidad de validar si hay transaccion

                da = new SqlDataAdapter(sqlCom);
                ds = new DataSet();

                da.Fill(ds);

                return ds;
            }
            catch
            {
                throw;
            }
            finally
            {
                Utilerias.DestruirDataAdapter(ref da);
                Utilerias.DestruirSqlCommand(ref sqlCom);

                if (cerrarConexion) { this.CerrarConexion(); }
            }
        }
        /// <summary>
        /// Ejecuta un procedimiento almacenado de consulta y obtiene los datos en un DataTable de forma asincronica
        /// </summary>
        /// <param name="nombreProcedimiento">Nombre del procedimiento</param>
        /// <param name="parametros">Parametros del procedimiento, los parametros no deben incluir la @</param>
        /// <returns><code>DataTable</code></returns>
        public async Task<DataTable> EjecutaConsultaTB(string nombreProcedimiento, params ParametroSql[] parametros)
        {
            DataTable tb;
            DataSet dst = null;

            try
            {
                dst = await this.EjecutaConsulta(nombreProcedimiento, parametros);

                if (dst == null) { return null; }

                tb = dst.Tables[0];

                dst.Tables.Remove(tb);

                return tb;
            }
            catch
            {
                throw;
            }
            finally
            {
                Utilerias.DestruirDataSet(ref dst);
            }
        }
        /// <summary>
        /// Ejecuta un procedimiento almacenado de consulta y obtiene los datos en un DataTable de forma sincronica
        /// </summary>
        /// <param name="nombreProcedimiento">Nombre del procedimiento</param>
        /// <param name="parametros">Parametros del procedimiento, los parametros no deben incluir la @</param>
        /// <returns><code>DataTable</code></returns>
        public DataTable EjecutaConsultaTBSync(string nombreProcedimiento, params ParametroSql[] parametros)
        {
            DataTable tb;
            DataSet dst = null;

            try
            {
                dst = this.EjecutaConsultaSync(nombreProcedimiento, parametros);

                if (dst == null) { return null; }

                tb = dst.Tables[0];

                dst.Tables.Remove(tb);

                return tb;
            }
            catch
            {
                throw;
            }
            finally
            {
                Utilerias.DestruirDataSet(ref dst);
            }
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado que no es para consulta de forma asincronica
        /// </summary>
        /// <param name="nombreProcedimiento">Nombre del procedimiento almacenado</param>
        /// <param name="parametros">Parametros del procedimiento almacenado</param>
        /// <returns>Retorna un tipo <code>Resultado</code>, este contiene el estatus de la ejecucion, valida o invalida. 
        /// Tambien contiene los parametros de salida (OutPut) si hay alguno.</returns>
        public async Task<Resultado> EjecutaSql(string nombreProcedimiento, params ParametroSql[] parametros)
        {
            bool cerrarConexion = false;
            SqlCommand sqlCom = null;
            Resultado resultado = new Resultado();

            try
            {
                resultado.Valido = false;

                if (!this.ConexionCreada)
                {
                    cerrarConexion = true;
                    if (!this.CrearConexion()) { return resultado; }
                }

                sqlCom = this.CrearComandoSql(nombreProcedimiento, parametros);
                sqlCom.Connection = this.Conexion;
                sqlCom.Transaction = this.Transaccion;
                sqlCom.CommandTimeout = 600;

                // Internamente valida si ya esta abierta, si lo esta no hace nada
                this.AbrirConexion();

                await sqlCom.ExecuteNonQueryAsync();

                var parametrosSalida = parametros.Where(p => p.InOutPut == true);

                // Validar si hay parametros que retornan valores. Si los hay, les asigna el valor regresado y los agrega al resultado.
                if (parametrosSalida.Count() > 0)
                {
                    foreach (ParametroSql parametro in parametrosSalida)
                    {
                        parametro.Valor = sqlCom.Parameters[parametro.Nombre].Value;
                    }

                    resultado.Parametros = parametrosSalida.ToArray();
                }

                resultado.Valido = true;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cerrarConexion) { this.CerrarConexion(); }

                Utilerias.DestruirSqlCommand(ref sqlCom);
            }

            return resultado;
        }
        #endregion


        #region Dispose
        /// <summary>
        /// Funcion de la implementacion de la interfas <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            if (this.Transaccion != null)
            {
                this.Transaccion.Rollback();
                this.Transaccion = null;
            }
            if (this.Conexion != null)
            {
                this.Conexion.Dispose();
                this.Conexion = null;
            }
        }
        #endregion

        #endregion

        #region Tipos Anidados
        /// <summary>
        /// Parametro necesario para la ejecucion de procedimientos almacenados con las funciones <see cref="ConexionBD.EjecutaSql(string, ParametroSql[])"/>, 
        /// <see cref="ConexionBD.EjecutaConsultaTBSync(string, ParametroSql[])"/>, <see cref="ConexionBD.EjecutaConsultaTB(string, ParametroSql[])"/>,
        /// <see cref="ConexionBD.EjecutaConsultaSync(string, ParametroSql[])"/>, <see cref="ConexionBD.EjecutaConsulta(string, ParametroSql[])"/> y
        /// <see cref="ConexionBD.RegresaDatoSQL{T}(string, T, ParametroSql[])"/>
        /// </summary>
        public class ParametroSql
        {
            /// <summary>
            /// Nombre del parametro, no incluir arroba
            /// </summary>
            public string Nombre { get; set; }
            /// <summary>
            /// Valor del parametro
            /// </summary>
            public object Valor { get; set; }
            /// <summary>
            /// Indica si el parametro es de entrara y salida. Cuando un parametro es de entrada y salida, despues de ejecutar el procedimiento
            /// podras obtener el valor de salida por medio de <see cref="Resultado"/>. <see cref="Resultado"/> solo se obteiene al ejecutar 
            /// procedimientos que no son consulta, debes utilizar la funcion <see cref="ConexionBD.EjecutaSql(string, ParametroSql[])"/>.
            /// </summary>
            public bool InOutPut { get; set; }

            /// <summary>
            /// Iniciarliza el parametro con los valores necesarios
            /// </summary>
            /// <param name="nombre">Nombre del parametro, no incluir arroba (@)</param>
            /// <param name="valor">Valor del parametro</param>
            public ParametroSql(string nombre, object valor)
            {
                Nombre = nombre;
                Valor = valor;
            }
            /// <summary>
            /// Iniciarliza el parametro con los valores necesarios
            /// </summary>
            /// <param name="nombre">Nombre del parametro, no incluir arroba (@)</param>
            /// <param name="valor">Valor del parametro</param>
            /// <param name="inOutPut">Indica el parametro sera de entrara y salida. Los parametros de entrada y salida solo se podra obtener 
            /// su valor de salida si se ejecuta la funcion <see cref="ConexionBD.EjecutaSql(string, ParametroSql[])"/>.</param>
            public ParametroSql(string nombre, object valor, bool inOutPut)
            {
                // No debe contener la arroba
                Nombre = nombre.Replace('@', ' ').Trim();
                Valor = valor;
                InOutPut = inOutPut;
            }
        }
        /// <summary>
        /// Contiene el resultado despues de haber ejecutado la funcion <see cref="ConexionBD.EjecutaSql(string, ParametroSql[])"/>
        /// </summary>
        public class Resultado
        {
            /// <summary>
            /// Indica si la ejecución del procedimiento fue exitosa.
            /// </summary>
            public bool Valido;
            /// <summary>
            /// Contiene los parametros que fueron indicados como de entrada y salida. <see cref="ParametroSql.InOutPut"/>
            /// </summary>
            public ParametroSql[] Parametros;
        }
        #endregion
    }
}