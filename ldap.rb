  require 'rubygems'
  require 'net/ldap'
  require 'highline/import'

  password = ask("Enter your password:  ") { |q| q.echo = false }
  ldap = Net::LDAP.new :host => "ldap.stuba.sk",
       :port => 389,
       :auth => {
             :method => :simple,
             :username => "uid=#{ARGV[0]},ou=People,dc=stuba,dc=sk",
             :password => "#{password}"
       }

  filter = Net::LDAP::Filter.eq("uid", "#{ARGV[0]}")
  treebase = "dc=stuba, dc=sk"

  ldap.search(:base => treebase, :filter => filter) do |entry|
    puts "DN: #{entry.dn}"
    entry.each do |attribute, values|
      puts "   #{attribute}:"
      values.each do |value|
        puts "      --->#{value}"
      end
    end
  end

  p ldap.get_operation_result

