﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLib.IOC
{
    public class ServiceContainer : IServiceContainer
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly Dictionary<Type, object> _createObjects = new Dictionary<Type, object>();
        public ServiceContainer(IServiceCollection serviceContainer)
        {
            if (serviceContainer == null)
            {
                throw new InvalidOperationException("Invalid service collection");
            }
            _serviceCollection = serviceContainer;
        }
        private object Create(Type type)
        {
            if(_serviceCollection.CheckType(type) == false)
            {
                throw new InvalidOperationException("There is not service key");
            }
            try
            {
                var typeInfo = _serviceCollection.GetType(type);

                var objectType = typeInfo.Type;
                var isSingleTon = typeInfo.IsSingleton;
                var cacheobject = typeInfo.Prameter;
                var callback= typeInfo.CalbakcFunc;
                var defaultConstructors = objectType!.GetConstructors();

                if (defaultConstructors.Count() <= 0 && cacheobject == null)
                    throw new InvalidCastException("Ther is no constructor");

                if(isSingleTon == false && callback == null && cacheobject == null)
                {
                    var defaultConstructor = defaultConstructors[0];
                    var defaultParams = defaultConstructor.GetParameters();
                    var parameter = defaultParams.Select(param => Create(param.ParameterType)).ToArray();
                    var service = defaultConstructor.Invoke(parameter);
                    if(service.GetType().IsSubclassOf(typeof(INotifyPropertyChanged)))
                    {
                        var _servics = service as INotifyPropertyChanged;
                    }
                    return service;
                }

                if(isSingleTon == false && callback != null & cacheobject == null)
                {
                    var methodeInfo = callback.GetType().GetMethod("Invoke");
                    var service = methodeInfo.Invoke(callback, new[] { this });
                    if(service.GetType().IsSubclassOf(typeof(INotifyPropertyChanged)))
                    {
                        var _service = service as INotifyPropertyChanged;                        
                    }
                    return service;
                }

                if(isSingleTon == true && callback != null && cacheobject == null)
                {
                    if(_createObjects.ContainsKey(type) == true)
                    {
                        return _createObjects[type];
                    }
                    var defaultConstructor = defaultConstructors[0];
                    var defaultParams = defaultConstructor.GetParameters();
                    var parameters = defaultParams.Select(param => Create(param.ParameterType)).ToArray();
                    var service = defaultConstructor.Invoke(parameters);
                    _createObjects[type] = service;
                    return service;
                }


                if (isSingleTon == true && callback != null && cacheobject == null)
                {
                    if (this._createObjects.ContainsKey(type) == true)
                        return this._createObjects[type];

                    var methodInfo = callback.GetType().GetMethod("Invoke");
                    var service = methodInfo.Invoke(callback, new[] { this });
                    _createObjects[type] = service;
                    //if (service.GetType().IsSubclassOf(typeof(NotifyObject)))
                    //{
                    //    var _service = service as NotifyObject;
                    //    _service.OnActive();
                    //}
                    return service;
                }

                if (isSingleTon == true && callback == null && cacheobject != null)
                {
                    if (this._createObjects.ContainsKey(type) == true)
                        return this._createObjects[type];

                    this._createObjects[type] = cacheobject;
                   
                    return cacheobject;
                }


                throw new InvalidOperationException("Invalid service collection infomation");

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unknown", ex);
            }
        }
        public TInterface GetService<TInterface>() where TInterface : class
        {
            try
            {
                return (TInterface)Create(typeof(TInterface));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unknown", ex);
            }
        }

        public object GetService(Type serviceType)
        {
            try
            {
                return Create(serviceType);
            }
            catch (Exception ex)
            {

                throw new InvalidOperationException("Unknown", ex);
            }
        }

        public Type KeyGet(string key)
        {
            return _serviceCollection.KeyType(key);
        }
    }
}
