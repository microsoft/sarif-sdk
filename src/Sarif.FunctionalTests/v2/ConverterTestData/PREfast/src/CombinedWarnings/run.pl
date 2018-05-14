my $PASSED  = 0;
my $FAILED  = 1;
my $CASCADE = 3;

my $source;
$source = $ENV{SOURCE};
my $expectLog;
$expectLog = $ENV{RESULTS};
my $xsdroot; 
$xsdroot = $ENV{XSDROOT};
my $flags;
$flags = $ENV{FLAGS};

my $logFile;
$logFile = $ENV{LOGFILE};

my $cmd;
$cmd = "stopit -pcl.exe##OK -a cl.exe -c -analyze -analyze:quiet- -W4 -analyze:log ..\\Xmls\\$logFile $source > defects.txt";

if(defined $ENV{FIRSTCHECK})
{
   if( -e "..\\Xmls\\$logFile" )
   {
      if ( unlink( "..\\Xmls\\$logFile" ) != 1 )
      {
          print "Error deleting existing xml file";
          exit($CASCADE);
      }
   }
}

print "Building: $cmd\n";
if (run_cmd($cmd)) {
  dumpLog();
  exit($CASCADE);
}

$cmd = "perl -S verify_prefast_warnings.pl $expectLog defects.txt";
print "Running script to verify warnings: $cmd\n";
my $returnCode = 0;
$returnCode = run_cmd($cmd);
if($returnCode != 0)
{
   exit $returnCode;
}

if(defined $ENV{XMLVALIDATION})
{
   $cmd =  $xsdroot . "\\XsdValidate.exe -xml ..\\Xmls\\$logFile";
   print "Running script to verify xml xsd conformance: $cmd\n";
   $returnCode = run_cmd($cmd);
   if($returnCode != 0)
   {
      exit $returnCode;
   }
   $cmd = "..\\ValidateOutput.exe -xmlbaseline ..\\ExpectedOutput\\$logFile -xmloutput ..\\Xmls\\$logFile $flags";
   print "Running script to verify xml data: $cmd\n";
   exit (run_cmd($cmd));
}
else
{
   exit $returnCode;
}

#
# run_cmd
# Run a command and redirect its stdout/stderr to a log file.
# Returns the exit code from the process.
#
sub run_cmd {
    my $cmd = shift;
    my $retval;

    $retval = system("$cmd") >> 8;

    return $retval;
}

#Dump the contents of defects.txt to screen, incase of cascade
sub dumpLog{
	open (DEFECTS, "defects.txt") or die ("can't open defects.txt");
	my $line;
	while ($line = <DEFECTS>)
	{
		print $line;
	}		
	close (DEFECTS);
	return 0;
}