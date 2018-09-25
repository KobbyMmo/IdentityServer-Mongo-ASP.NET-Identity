﻿using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServerSample.Interface;
using IdentityServerSample.Models;
using IdentityServerSample.Repository;
using IdentityServerSample.Store;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerSample.Extensions
{
    public static class IdentityServerBuilderExtensions
    {
        /// <summary>
        /// Adds mongo repository (mongodb) for IdentityServer
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddMongoRepository(this IIdentityServerBuilder builder)
        {
            builder.Services.AddTransient<IRepository, MongoRepository>();
            return builder;
        }

        /// <summary>
        /// Adds mongodb implementation for the "Asp Net Core Identity" part (saving user and roles)
        /// </summary>
        /// <remarks><![CDATA[
        /// Contains implemenations for
        /// - IUserStore<T>
        /// - IRoleStore<T>
        /// ]]></remarks>
        public static IIdentityServerBuilder AddMongoDbForAspIdentity<TIdentity, TRole>(this IIdentityServerBuilder builder, IConfigurationRoot configuration) where
            TIdentity : ApplicationUser where TRole : Microsoft.AspNetCore.Identity.MongoDB.IdentityRole
        {

            //User Mongodb for Asp.net identity in order to get users stored
            var configurationOptions = configuration.Get<ConfigurationOptions>();
            var client = new MongoClient(configurationOptions.MongoConnection);
            var database = client.GetDatabase(configurationOptions.MongoDatabaseName);



            // Configure Asp Net Core Identity / Role to use MongoDB
            builder.Services.AddSingleton<IUserStore<TIdentity>>(x =>
            {
                var usersCollection = database.GetCollection<TIdentity>("Identity_Users");
                IndexChecks.EnsureUniqueIndexOnNormalizedEmail(usersCollection);
                IndexChecks.EnsureUniqueIndexOnNormalizedUserName(usersCollection);
                return new UserStore<TIdentity>(usersCollection);
            });

            builder.Services.AddSingleton<IRoleStore<TRole>>(x =>
            {
                var rolesCollection = database.GetCollection<TRole>("Identity_Roles");
                IndexChecks.EnsureUniqueIndexOnNormalizedRoleName(rolesCollection);
                return new RoleStore<TRole>(rolesCollection);
            });
            builder.Services.AddIdentity<TIdentity, TRole>().AddDefaultTokenProviders();


            return builder;
        }

        /// <summary>
        /// Configure ClientId / Secrets
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configurationOption"></param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddClients(this IIdentityServerBuilder builder)
        {
            builder.Services.AddTransient<IClientStore, CustomClientStore>();
            builder.Services.AddTransient<ICorsPolicyService, InMemoryCorsPolicyService>();
            return builder;
        }


        /// <summary>
        /// Configure API  &  Resources
        /// Note: Api's have also to be configured for clients as part of allowed scope for a given clientID 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddIdentityApiResources(this IIdentityServerBuilder builder)
        {
            builder.Services.AddTransient<IResourceStore, CustomResourceStore>();
            return builder;
        }

        /// <summary>
        /// Configure Grants
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddPersistedGrants(this IIdentityServerBuilder builder)
        {
            builder.Services.TryAddSingleton<IPersistedGrantStore, CustomPersistedGrantStore>();
            return builder;
        }

    }
}
