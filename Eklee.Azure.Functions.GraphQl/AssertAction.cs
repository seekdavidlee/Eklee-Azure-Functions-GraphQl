﻿namespace Eklee.Azure.Functions.GraphQl
{
	/// <summary>
	/// Related actions that is supported.
	/// </summary>
	public enum AssertAction
	{
		BatchCreateOrUpdate,
		BatchCreate,
		Create,
		CreateOrUpdate,
		Update,
		Delete,
		DeleteAll
	}
}