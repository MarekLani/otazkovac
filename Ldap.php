<?php   if ( ! defined('BASEPATH')) exit('No direct script access allowed');
/**
 * Kniznica pre zabezpecenie prihlasovania cez LDAP protokol
 * 
 * @author Ivan Srba
 * @version 1.0
 */
class Ldap {
	public function authentificate($username, $password)
	{
		$CI =& get_instance();
		$CI->load->library('config');
		
		$CI->config->load('ldap');
		$ldap = $CI->config->item('ldap');	

		$ds = @ldap_connect($ldap['host'], $ldap['port']);

		@ldap_set_option($ds, LDAP_OPT_PROTOCOL_VERSION, 3);

		
		$ldapBindResult = @ldap_bind($ds, 'uid='.$username.','.$ldap['basedn'], $password);

		if (!$ldapBindResult)
		{
			@ldap_close($ds);
			return NULL;
		}

		$ldapFilter = array("uid", "userPassword", "employeetype", "uisid", "cn", "sn", "givenname");

		$ldapSearchResult = @ldap_search($ds, $ldap['basedn'], 'uid='.$username, $ldapFilter);
		
		if ($ldapSearchResult)
		{
			$result = @ldap_get_entries($ds, $ldapSearchResult);
		}
		else
		{
			@ldap_close($ds);
			return NULL;
		}
		@ldap_close($ds);

		return $result;
	}
}
