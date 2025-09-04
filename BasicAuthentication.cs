using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using ERPServer.Models;
using Bharuwa.Erp.Common;
using ImsPosLibraryCore.Models;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System;
using System.Linq;
using Dapper;
using DocumentFormat.OpenXml.InkML;
using System.Data;
using Bharuwa.Erp.Data;
using Bharuwa.Erp.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Bharuwa.Erp.Auth.Services;
using Bharuwa.Erp.Queries;

namespace ERPServer
{
    public class BasicAuthentication : TypeFilterAttribute
    {
        public BasicAuthentication() : base(typeof(BasicAuthenticationFilter))
        {
            Order = -1;
        }


        private class BasicAuthenticationFilter : IAsyncAuthorizationFilter
        {
            public BasicAuthenticationFilter()
            {

            }

            public async Task OnAuthorizationAsync(AuthorizationFilterContext _context)
            {
                IDbContext _dbContext = _context.HttpContext.RequestServices.GetService(typeof(IDbContext)) as IDbContext;
                IInitialDal _initialDal = _context.HttpContext.RequestServices.GetService(typeof(IInitialDal)) as IInitialDal;
                var endpoint = _context.HttpContext.GetEndpoint();
                var allowAnonymous = endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null;

                if (allowAnonymous)
                {
                    var cress = await _initialDal.getFirmConnectionDetails();
                    string connectionstring = cress.Item1;
                    string MasterDBConnectionString = _initialDal.getMasterDbConnectionString();
                    string UserConnectionString = cress.userConnectionString;
                    string LogDBConnectionString = cress.logDbConnectionString;
                    await Task.CompletedTask;
                    _dbContext.setDbContextforallow(MasterDBConnectionString, UserConnectionString, "admin", "", LogDBConnectionString, (DataClientEnum)cress.databaseClient);
                    return;
                }
                Microsoft.Extensions.Primitives.StringValues token;
                _context.HttpContext.Request.Headers.TryGetValue("Authorization", out token);
                string t = token.ToString().Replace("Bearer ", String.Empty);

                _context.HttpContext.Request.Headers.TryGetValue("X-Browseridentity-Token", out token);
                string BrowserIdentityToken = token.ToString();
                var cres = await _initialDal.getFirmConnectionDetails();
                string conString = cres.Item1;
                using (IDbConnection con = _initialDal.GetConnection(cres.Item2, conString))
                {
                    JwtTokenDetails tokenDetails = await con.QueryFirstOrDefaultAsync<JwtTokenDetails>("Select BrowserIdentityToken, Expires_In, userprofiles,settings from JwtTokenDetails where token =@token", new { token = t });
                    if (tokenDetails != null)
                    {

                        if (string.Compare(BrowserIdentityToken, tokenDetails.BrowserIdentityToken, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (DateTime.Compare(tokenDetails.Expires_In, DateTime.UtcNow) < 0)
                            {
                                _context.Result = new UnauthorizedResult();
                            }
                            else
                            {
                                SystemSettings Settings = JsonConvert.DeserializeObject<SystemSettings>(tokenDetails.settings);
                                UserProfile userProfiles = JsonConvert.DeserializeObject<UserProfile>(tokenDetails.userprofiles);
                                string MasterDBConnectionString = _initialDal.getMasterDbConnectionString();
                                string UserConnectionString = cres.userConnectionString;
                                string LogDBConnectionString = cres.logDbConnectionString;

                                string storageLocation = await con.ExecuteScalarAsync<string>(FileManager.GetQuery(DataQuery.GetStorageLocation), new { CompanyId = userProfiles.CompanyInfo.COMPANYID });
                                _dbContext.setDbContext(MasterDBConnectionString, UserConnectionString, userProfiles.username, userProfiles.email, Settings, userProfiles, userProfiles.EmployeeId, LogDBConnectionString, (DataClientEnum)cres.databaseClient, userProfiles.CompanyInfo.COMPANYID, storageLocation, t);

                            }
                        }
                        else
                        {
                            _context.Result = new UnauthorizedResult();
                        }
                    }
                    else
                    {
                        _context.Result = new UnauthorizedResult();
                    }
                }
            }
        }
    }
}