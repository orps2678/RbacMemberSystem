//System
global using System.ComponentModel.DataAnnotations;
global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using System.Text;

// Microsoft
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.Extensions.Options;

// Third Party
global using FluentValidation;
global using Serilog;

// Project（等建立檔案後再取消註解）
global using RbacMemberSystem.Api.Entities;
global using RbacMemberSystem.Api.Data;
global using RbacMemberSystem.Api.DTOs.Auth;
global using RbacMemberSystem.Api.Data.Configurations;
global using RbacMemberSystem.Api.Validators.Auth;
global using RbacMemberSystem.Api.Services;
global using RbacMemberSystem.Api.Configurations;
