# CountryBlock

Blocks entire Countries in Windows

## Permissions

Because Changes in the System Firewall affect all Users and Services,
you need administrative Rights for this Application to execute.
Keep this in Mind if you use it in your Scripts.

## Usage

    CountryBlock.exe {/add|/remove|/addr} <country> [/dir {in|out}] | /countries | /rules
    
    /add         - Adds the specified country to the list
    /remove      - Removes the specified country from the list
                   (use 'ALL' to remove all)
    /addr        - Shows all addresses of the specified country
    /countries   - Lists all countries
    /rules       - Lists all blocked countries
    country      - Country to (un-)block (2 letter country code)
    /dir in|out  - Direction of connection to block.
                   In most cases you only want 'in'
                   Not specifying any direction (un-)blocks both.
                   Only has an effect on /add and /remove command.

## Adding existing Countries

If you add a Country the Application will first remove all Country Entries.
It is therefore safe to add an existing Country.
You can use this to switch between Rule Directions.
You don't need to remove the Country first.

If you blocked `US` in both Directions but only want to block Inbound,
these two calls are identical:

    CountryBlock /add US /dir In
    CountryBlock /remove US /dir Out

## Panic Mode

You can use the `/PANIC` Argument (case sensitive) which will remove all CountryBlock Rules.
If the Argument is present,
the Application will not evaluate any other Arguments and simply removes all Country Entries.
You can use this if you blocked the API and removed the Cache File.

# Rule Names

The Rules in the Firewall are named in this Pattern (without Braces):

    CountryBlock-{In|Out}-{CC}

## Firewall Limitations

It's no Mistake if you see the same rule name multiple times.
The Windows Firewall has an upper Limit of 1000 IP Entries for each Rule.
CountryBlock will create multiple Rules with the same Name in this Case.

# API

The CountryBlock by Default uses the API from https://cable.ayra.ch/ip/.

# IP Version

At the Moment, this Application only supports IPv4

# Cache

The Application creates a Cache File called cache.json.
The File is created when it is needed for the first Time.
it contains all Countries with all Subnets.
Feel free to use it for other Purposes.
See the "IP List" Chapter below before publishing it.

## Stateless operation

This Application is stateless.
It doesn't keeps track of the countries that are blocked,
instead when requesting the list of blocked Countries (`/rules`)
it will scan through the firewall rules and pick them according to the Name format (see "Rule Names" above).
This means it is safe to add/edit/remove the Firewall Rules manually.
Be aware that any edited Rules are replaced when the Country is added again.

# Side effects of blocking certain Ranges

You should be careful when blocking certain Ranges.
in general there is no guarantee that an IP address is allocated to the country the user actually visits from.
This is possible if the Address is owned by a Multi-national ISP.

## CH

The default API is hosted in Switzerland.
If you block this Range and delete the Cache File you will no longer be able to obtain it.
The Application will no longer run and you need to manually remove the outbound Rules for `CH`.
Also see "Panic Mode" for Help in this Case.

## US

US is only mainland USA including Hawaii and Alaska.
This applies to all Countries that have territories.

## __

This Range contains a mixture of Addresses:

- Reserved Addresses
- Unassigned Addresses
- Private Addresses
- Broadcast Address

If you block this Range your entire Connection is very likely to stop working.
If this happens, simply unblock the Range again and request a new IP from your DHCP Server.
Type `ipconfig /release && ipconfig /renew` in your Terminal for this.
As an alternative, reboot your device after removing the Rule.
If you can't unblock because the cache file is missing, see the "Panic Mode" Chapter above

# IP List

The IP List in the API is from [ip2location](https://lite.ip2location.com/terms-of-use).
It only uses the free version. If you publish the cache file you are required to give credit.
Check the "GRANT OF RIGHTS" chapter in their TOS.

