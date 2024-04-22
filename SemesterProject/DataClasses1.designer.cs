﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SemesterProject
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="StoreDB")]
	public partial class DataClasses1DataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertCUSTOMER(CUSTOMER instance);
    partial void UpdateCUSTOMER(CUSTOMER instance);
    partial void DeleteCUSTOMER(CUSTOMER instance);
    partial void InsertSTORE_ITEM(STORE_ITEM instance);
    partial void UpdateSTORE_ITEM(STORE_ITEM instance);
    partial void DeleteSTORE_ITEM(STORE_ITEM instance);
    partial void InsertPURCHASE_STORE_ITEM(PURCHASE_STORE_ITEM instance);
    partial void UpdatePURCHASE_STORE_ITEM(PURCHASE_STORE_ITEM instance);
    partial void DeletePURCHASE_STORE_ITEM(PURCHASE_STORE_ITEM instance);
    partial void InsertPURCHASE(PURCHASE instance);
    partial void UpdatePURCHASE(PURCHASE instance);
    partial void DeletePURCHASE(PURCHASE instance);
    #endregion
		
		public DataClasses1DataContext() : 
				base(global::SemesterProject.Properties.Settings.Default.StoreDBConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public DataClasses1DataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DataClasses1DataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DataClasses1DataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DataClasses1DataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<CUSTOMER> CUSTOMERs
		{
			get
			{
				return this.GetTable<CUSTOMER>();
			}
		}
		
		public System.Data.Linq.Table<STORE_ITEM> STORE_ITEMs
		{
			get
			{
				return this.GetTable<STORE_ITEM>();
			}
		}
		
		public System.Data.Linq.Table<PURCHASE_STORE_ITEM> PURCHASE_STORE_ITEMs
		{
			get
			{
				return this.GetTable<PURCHASE_STORE_ITEM>();
			}
		}
		
		public System.Data.Linq.Table<PURCHASE> PURCHASEs
		{
			get
			{
				return this.GetTable<PURCHASE>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.CUSTOMER")]
	public partial class CUSTOMER : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _CustomerId;
		
		private string _LoginId;
		
		private string _Password;
		
		private decimal _Balance;
		
		private EntitySet<PURCHASE> _PURCHASEs;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnCustomerIdChanging(int value);
    partial void OnCustomerIdChanged();
    partial void OnLoginIdChanging(string value);
    partial void OnLoginIdChanged();
    partial void OnPasswordChanging(string value);
    partial void OnPasswordChanged();
    partial void OnBalanceChanging(decimal value);
    partial void OnBalanceChanged();
    #endregion
		
		public CUSTOMER()
		{
			this._PURCHASEs = new EntitySet<PURCHASE>(new Action<PURCHASE>(this.attach_PURCHASEs), new Action<PURCHASE>(this.detach_PURCHASEs));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_CustomerId", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int CustomerId
		{
			get
			{
				return this._CustomerId;
			}
			set
			{
				if ((this._CustomerId != value))
				{
					this.OnCustomerIdChanging(value);
					this.SendPropertyChanging();
					this._CustomerId = value;
					this.SendPropertyChanged("CustomerId");
					this.OnCustomerIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_LoginId", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string LoginId
		{
			get
			{
				return this._LoginId;
			}
			set
			{
				if ((this._LoginId != value))
				{
					this.OnLoginIdChanging(value);
					this.SendPropertyChanging();
					this._LoginId = value;
					this.SendPropertyChanged("LoginId");
					this.OnLoginIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Password", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string Password
		{
			get
			{
				return this._Password;
			}
			set
			{
				if ((this._Password != value))
				{
					this.OnPasswordChanging(value);
					this.SendPropertyChanging();
					this._Password = value;
					this.SendPropertyChanged("Password");
					this.OnPasswordChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Balance", DbType="Money NOT NULL")]
		public decimal Balance
		{
			get
			{
				return this._Balance;
			}
			set
			{
				if ((this._Balance != value))
				{
					this.OnBalanceChanging(value);
					this.SendPropertyChanging();
					this._Balance = value;
					this.SendPropertyChanged("Balance");
					this.OnBalanceChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="CUSTOMER_PURCHASE", Storage="_PURCHASEs", ThisKey="CustomerId", OtherKey="CustomerId")]
		public EntitySet<PURCHASE> PURCHASEs
		{
			get
			{
				return this._PURCHASEs;
			}
			set
			{
				this._PURCHASEs.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_PURCHASEs(PURCHASE entity)
		{
			this.SendPropertyChanging();
			entity.CUSTOMER = this;
		}
		
		private void detach_PURCHASEs(PURCHASE entity)
		{
			this.SendPropertyChanging();
			entity.CUSTOMER = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.STORE_ITEM")]
	public partial class STORE_ITEM : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _StoreItemId;
		
		private string _Manufacturer;
		
		private string _ProductName;
		
		private int _QuantityAvailable;
		
		private decimal _Price;
		
		private string _ImageUrl;
		
		private EntitySet<PURCHASE_STORE_ITEM> _PURCHASE_STORE_ITEMs;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnStoreItemIdChanging(int value);
    partial void OnStoreItemIdChanged();
    partial void OnManufacturerChanging(string value);
    partial void OnManufacturerChanged();
    partial void OnProductNameChanging(string value);
    partial void OnProductNameChanged();
    partial void OnQuantityAvailableChanging(int value);
    partial void OnQuantityAvailableChanged();
    partial void OnPriceChanging(decimal value);
    partial void OnPriceChanged();
    partial void OnImageUrlChanging(string value);
    partial void OnImageUrlChanged();
    #endregion
		
		public STORE_ITEM()
		{
			this._PURCHASE_STORE_ITEMs = new EntitySet<PURCHASE_STORE_ITEM>(new Action<PURCHASE_STORE_ITEM>(this.attach_PURCHASE_STORE_ITEMs), new Action<PURCHASE_STORE_ITEM>(this.detach_PURCHASE_STORE_ITEMs));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_StoreItemId", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int StoreItemId
		{
			get
			{
				return this._StoreItemId;
			}
			set
			{
				if ((this._StoreItemId != value))
				{
					this.OnStoreItemIdChanging(value);
					this.SendPropertyChanging();
					this._StoreItemId = value;
					this.SendPropertyChanged("StoreItemId");
					this.OnStoreItemIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Manufacturer", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string Manufacturer
		{
			get
			{
				return this._Manufacturer;
			}
			set
			{
				if ((this._Manufacturer != value))
				{
					this.OnManufacturerChanging(value);
					this.SendPropertyChanging();
					this._Manufacturer = value;
					this.SendPropertyChanged("Manufacturer");
					this.OnManufacturerChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ProductName", DbType="NVarChar(100) NOT NULL", CanBeNull=false)]
		public string ProductName
		{
			get
			{
				return this._ProductName;
			}
			set
			{
				if ((this._ProductName != value))
				{
					this.OnProductNameChanging(value);
					this.SendPropertyChanging();
					this._ProductName = value;
					this.SendPropertyChanged("ProductName");
					this.OnProductNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_QuantityAvailable", DbType="Int NOT NULL")]
		public int QuantityAvailable
		{
			get
			{
				return this._QuantityAvailable;
			}
			set
			{
				if ((this._QuantityAvailable != value))
				{
					this.OnQuantityAvailableChanging(value);
					this.SendPropertyChanging();
					this._QuantityAvailable = value;
					this.SendPropertyChanged("QuantityAvailable");
					this.OnQuantityAvailableChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Price", DbType="Money NOT NULL")]
		public decimal Price
		{
			get
			{
				return this._Price;
			}
			set
			{
				if ((this._Price != value))
				{
					this.OnPriceChanging(value);
					this.SendPropertyChanging();
					this._Price = value;
					this.SendPropertyChanged("Price");
					this.OnPriceChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ImageUrl", DbType="NVarChar(MAX)")]
		public string ImageUrl
		{
			get
			{
				return this._ImageUrl;
			}
			set
			{
				if ((this._ImageUrl != value))
				{
					this.OnImageUrlChanging(value);
					this.SendPropertyChanging();
					this._ImageUrl = value;
					this.SendPropertyChanged("ImageUrl");
					this.OnImageUrlChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="STORE_ITEM_PURCHASE_STORE_ITEM", Storage="_PURCHASE_STORE_ITEMs", ThisKey="StoreItemId", OtherKey="StoreItemId")]
		public EntitySet<PURCHASE_STORE_ITEM> PURCHASE_STORE_ITEMs
		{
			get
			{
				return this._PURCHASE_STORE_ITEMs;
			}
			set
			{
				this._PURCHASE_STORE_ITEMs.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_PURCHASE_STORE_ITEMs(PURCHASE_STORE_ITEM entity)
		{
			this.SendPropertyChanging();
			entity.STORE_ITEM = this;
		}
		
		private void detach_PURCHASE_STORE_ITEMs(PURCHASE_STORE_ITEM entity)
		{
			this.SendPropertyChanging();
			entity.STORE_ITEM = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.PURCHASE_STORE_ITEM")]
	public partial class PURCHASE_STORE_ITEM : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _PurchaseId;
		
		private int _StoreItemId;
		
		private EntityRef<STORE_ITEM> _STORE_ITEM;
		
		private EntityRef<PURCHASE> _PURCHASE;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnPurchaseIdChanging(int value);
    partial void OnPurchaseIdChanged();
    partial void OnStoreItemIdChanging(int value);
    partial void OnStoreItemIdChanged();
    #endregion
		
		public PURCHASE_STORE_ITEM()
		{
			this._STORE_ITEM = default(EntityRef<STORE_ITEM>);
			this._PURCHASE = default(EntityRef<PURCHASE>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PurchaseId", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int PurchaseId
		{
			get
			{
				return this._PurchaseId;
			}
			set
			{
				if ((this._PurchaseId != value))
				{
					if (this._PURCHASE.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnPurchaseIdChanging(value);
					this.SendPropertyChanging();
					this._PurchaseId = value;
					this.SendPropertyChanged("PurchaseId");
					this.OnPurchaseIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_StoreItemId", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int StoreItemId
		{
			get
			{
				return this._StoreItemId;
			}
			set
			{
				if ((this._StoreItemId != value))
				{
					if (this._STORE_ITEM.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnStoreItemIdChanging(value);
					this.SendPropertyChanging();
					this._StoreItemId = value;
					this.SendPropertyChanged("StoreItemId");
					this.OnStoreItemIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="STORE_ITEM_PURCHASE_STORE_ITEM", Storage="_STORE_ITEM", ThisKey="StoreItemId", OtherKey="StoreItemId", IsForeignKey=true)]
		public STORE_ITEM STORE_ITEM
		{
			get
			{
				return this._STORE_ITEM.Entity;
			}
			set
			{
				STORE_ITEM previousValue = this._STORE_ITEM.Entity;
				if (((previousValue != value) 
							|| (this._STORE_ITEM.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._STORE_ITEM.Entity = null;
						previousValue.PURCHASE_STORE_ITEMs.Remove(this);
					}
					this._STORE_ITEM.Entity = value;
					if ((value != null))
					{
						value.PURCHASE_STORE_ITEMs.Add(this);
						this._StoreItemId = value.StoreItemId;
					}
					else
					{
						this._StoreItemId = default(int);
					}
					this.SendPropertyChanged("STORE_ITEM");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="PURCHASE_PURCHASE_STORE_ITEM", Storage="_PURCHASE", ThisKey="PurchaseId", OtherKey="PurchaseId", IsForeignKey=true, DeleteOnNull=true, DeleteRule="CASCADE")]
		public PURCHASE PURCHASE
		{
			get
			{
				return this._PURCHASE.Entity;
			}
			set
			{
				PURCHASE previousValue = this._PURCHASE.Entity;
				if (((previousValue != value) 
							|| (this._PURCHASE.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._PURCHASE.Entity = null;
						previousValue.PURCHASE_STORE_ITEMs.Remove(this);
					}
					this._PURCHASE.Entity = value;
					if ((value != null))
					{
						value.PURCHASE_STORE_ITEMs.Add(this);
						this._PurchaseId = value.PurchaseId;
					}
					else
					{
						this._PurchaseId = default(int);
					}
					this.SendPropertyChanged("PURCHASE");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.PURCHASE")]
	public partial class PURCHASE : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _PurchaseId;
		
		private int _CustomerId;
		
		private int _Quantity;
		
		private decimal _UnitPrice;
		
		private System.DateTime _PurchaseDateTime;
		
		private EntitySet<PURCHASE_STORE_ITEM> _PURCHASE_STORE_ITEMs;
		
		private EntityRef<CUSTOMER> _CUSTOMER;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnPurchaseIdChanging(int value);
    partial void OnPurchaseIdChanged();
    partial void OnCustomerIdChanging(int value);
    partial void OnCustomerIdChanged();
    partial void OnQuantityChanging(int value);
    partial void OnQuantityChanged();
    partial void OnUnitPriceChanging(decimal value);
    partial void OnUnitPriceChanged();
    partial void OnPurchaseDateTimeChanging(System.DateTime value);
    partial void OnPurchaseDateTimeChanged();
    #endregion
		
		public PURCHASE()
		{
			this._PURCHASE_STORE_ITEMs = new EntitySet<PURCHASE_STORE_ITEM>(new Action<PURCHASE_STORE_ITEM>(this.attach_PURCHASE_STORE_ITEMs), new Action<PURCHASE_STORE_ITEM>(this.detach_PURCHASE_STORE_ITEMs));
			this._CUSTOMER = default(EntityRef<CUSTOMER>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PurchaseId", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int PurchaseId
		{
			get
			{
				return this._PurchaseId;
			}
			set
			{
				if ((this._PurchaseId != value))
				{
					this.OnPurchaseIdChanging(value);
					this.SendPropertyChanging();
					this._PurchaseId = value;
					this.SendPropertyChanged("PurchaseId");
					this.OnPurchaseIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_CustomerId", DbType="Int NOT NULL")]
		public int CustomerId
		{
			get
			{
				return this._CustomerId;
			}
			set
			{
				if ((this._CustomerId != value))
				{
					if (this._CUSTOMER.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnCustomerIdChanging(value);
					this.SendPropertyChanging();
					this._CustomerId = value;
					this.SendPropertyChanged("CustomerId");
					this.OnCustomerIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Quantity", DbType="Int NOT NULL")]
		public int Quantity
		{
			get
			{
				return this._Quantity;
			}
			set
			{
				if ((this._Quantity != value))
				{
					this.OnQuantityChanging(value);
					this.SendPropertyChanging();
					this._Quantity = value;
					this.SendPropertyChanged("Quantity");
					this.OnQuantityChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_UnitPrice", DbType="Money NOT NULL")]
		public decimal UnitPrice
		{
			get
			{
				return this._UnitPrice;
			}
			set
			{
				if ((this._UnitPrice != value))
				{
					this.OnUnitPriceChanging(value);
					this.SendPropertyChanging();
					this._UnitPrice = value;
					this.SendPropertyChanged("UnitPrice");
					this.OnUnitPriceChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PurchaseDateTime", DbType="DateTime2 NOT NULL")]
		public System.DateTime PurchaseDateTime
		{
			get
			{
				return this._PurchaseDateTime;
			}
			set
			{
				if ((this._PurchaseDateTime != value))
				{
					this.OnPurchaseDateTimeChanging(value);
					this.SendPropertyChanging();
					this._PurchaseDateTime = value;
					this.SendPropertyChanged("PurchaseDateTime");
					this.OnPurchaseDateTimeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="PURCHASE_PURCHASE_STORE_ITEM", Storage="_PURCHASE_STORE_ITEMs", ThisKey="PurchaseId", OtherKey="PurchaseId")]
		public EntitySet<PURCHASE_STORE_ITEM> PURCHASE_STORE_ITEMs
		{
			get
			{
				return this._PURCHASE_STORE_ITEMs;
			}
			set
			{
				this._PURCHASE_STORE_ITEMs.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="CUSTOMER_PURCHASE", Storage="_CUSTOMER", ThisKey="CustomerId", OtherKey="CustomerId", IsForeignKey=true)]
		public CUSTOMER CUSTOMER
		{
			get
			{
				return this._CUSTOMER.Entity;
			}
			set
			{
				CUSTOMER previousValue = this._CUSTOMER.Entity;
				if (((previousValue != value) 
							|| (this._CUSTOMER.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._CUSTOMER.Entity = null;
						previousValue.PURCHASEs.Remove(this);
					}
					this._CUSTOMER.Entity = value;
					if ((value != null))
					{
						value.PURCHASEs.Add(this);
						this._CustomerId = value.CustomerId;
					}
					else
					{
						this._CustomerId = default(int);
					}
					this.SendPropertyChanged("CUSTOMER");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_PURCHASE_STORE_ITEMs(PURCHASE_STORE_ITEM entity)
		{
			this.SendPropertyChanging();
			entity.PURCHASE = this;
		}
		
		private void detach_PURCHASE_STORE_ITEMs(PURCHASE_STORE_ITEM entity)
		{
			this.SendPropertyChanging();
			entity.PURCHASE = null;
		}
	}
}
#pragma warning restore 1591
