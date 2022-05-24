# NAT network configure

* *Subnet A* (20 hosts)
* *Subnet B* (2 hosts)
* *R1 and R2* - Cisco IOS operating system
* *PC1 and PC2* - Alpine Linux operating system

## R1 configuration

LAN settings
```bash
configure terminal
interface gigabitEthernet 0/0
ip address 192.168.123.1 255.255.255.252
no shutdown
exit	
```

WAN settings. Mask allows to use 30 subnet hosts.
```bash
interface gigabitEthernet 1/0
ip address 192.168.123.254 255.255.255.224
no shutdown
exit
```

Routing table setup
```bash
ip route 0.0.0.0 0.0.0.0 192.168.123.253
```

DHCP pool
```bash
ip dhcp excluded-address 192.168.123.1 192.168.123.10
ip dhcp pool r1_home
network 192.168.123.0 255.255.255.224
default-router 192.168.123.1
dns-server 8.8.8.8 4.4.4.4
exit
```

## R2 configuration

WAN settings
```bash
configure terminal
interface gigabitEthernet 0/0
ip address dhcp
no shutdown
exit
```

LAN settings
```bash
interface gigabitEthernet 1/0
ip address 192.168.123.253 255.255.255.252
no shutdown
exit
```

Routing table setup
```bash
ip route 192.168.123.0 255.255.255.0 gigabitEthernet 1/0
```

NAT settings
```bash
access-list 1 permit 192.168.123.0 0.0.0.255
interface gigabitEthernet 1/0
ip nat inside
interface gigabitEthernet 0/0
ip nat outside
ip nat inside source list 100 interface gigabitEthernet 0/0 overload
end
```

## PC1 configuration

```bash
udhcpc
```

## PC2 configuration

```bash
udhcpc
```