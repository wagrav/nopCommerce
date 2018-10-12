#!/bin/bash
# setting colors for output
green=`tput setaf 2`
reset=`tput sgr0`

folder=AppBuild
basefolder=$(pwd)
buildfolder=$(pwd)/$folder
#echo $basefolder
#echo $buildfolder
#echo "------------------------------------------------------------------"
if [ -d $buildfolder ];
then
echo "${green}Delete and create $folder dir${reset}"
rm -r $folder
mkdir $folder
else
echo "${green}Create $folder dir${reset}"
mkdir $folder
fi


#echo "Create dir"
#mkdir Build
echo "${green}Copy *.csproj files${reset}"
mkdir -p $buildfolder/Libraries/Nop.Core/
cp $basefolder/src/Libraries/Nop.Core/*.csproj $buildfolder/Libraries/Nop.Core/

mkdir -p $buildfolder/Libraries/Nop.Data/                          
cp $basefolder/src/Libraries/Nop.Data/*.csproj $buildfolder/Libraries/Nop.Data/ 
 
mkdir -p $buildfolder/Libraries/Nop.Services/                        
cp $basefolder/src/Libraries/Nop.Services/*.csproj $buildfolder/Libraries/Nop.Services/

mkdir -p $buildfolder/Presentation/Nop.Web.Framework/                   
cp $basefolder/src/Presentation/Nop.Web.Framework/*.csproj $buildfolder/Presentation/Nop.Web.Framework/
  
mkdir -p $buildfolder/Build/
cp $basefolder/src/Build/*.proj $buildfolder/Build/ 

mkdir -p $buildfolder/Presentation/Nop.Web/                                                
cp $basefolder/src/Presentation/Nop.Web/*.csproj $buildfolder/Presentation/Nop.Web/                      

echo ""
echo "${green}Restore${reset}"
cd $buildfolder/Presentation/Nop.Web 
/usr/bin/dotnet restore 

echo "" 
echo "${green}Copy other files${reset}"
cp -r $basefolder/src/Libraries/Nop.Core/ $buildfolder/Libraries/
rm -r $buildfolder/Libraries/Nop.Core/bin
rm -r $buildfolder/Libraries/Nop.Core/obj
                   
cp -r $basefolder/src/Libraries/Nop.Data/ $buildfolder/Libraries/
rm -r $buildfolder/Libraries/Nop.Data/bin
rm -r $buildfolder/Libraries/Nop.Data/obj 
                        
cp -r $basefolder/src/Libraries/Nop.Services/ $buildfolder/Libraries/
rm -r $buildfolder/Libraries/Nop.Services/bin
rm -r $buildfolder/Libraries/Nop.Services/obj 
                 
cp -r $basefolder/src/Presentation/Nop.Web.Framework/ $buildfolder/Presentation/
rm -r $buildfolder/Presentation/Nop.Web.Framework/bin
rm -r $buildfolder/Presentation/Nop.Web.Framework/obj 

cp -r $basefolder/src/Build/ $buildfolder/
                                    
cp -r $basefolder/src/Presentation/Nop.Web/ $buildfolder/Presentation/    
rm -r $buildfolder/Presentation/Nop.Web/Plugins/
mkdir -p $buildfolder/Presentation/Nop.Web/Plugins/bin  
touch $buildfolder/Presentation/Nop.Web/Plugins/bin/placeholder.txt

cd $buildfolder/Presentation/Nop.Web 

echo ""
echo "${green}Publish${reset}"
echo ""

if [ -d $basefolder/out ];
then
echo "${green}Delete and create out dir${reset}"
rm -r $basefolder/out
mkdir $basefolder/out
else
echo "${green}Create out dir${reset}"
mkdir $basefolder/out
fi

cd $buildfolder/Presentation/Nop.Web/ 
/usr/bin/dotnet publish -c Release -o $basefolder/out 

mkdir -p $basefolder/out/bin
mkdir -p $basefolder/out/log

echo ""
echo "${green}Resore plugins${reset}"
rm -r $basefolder/src/Presentation/Nop.Web/Plugins/
mkdir -p $basefolder/src/Presentation/Nop.Web/Plugins/bin  
touch $basefolder/src/Presentation/Nop.Web/Plugins/bin/placeholder.txt
for f in $basefolder/src/Plugins/*; do
    if [ -d ${f} ]; then
        #plugin=basename $f
        echo ""
        echo "${green}Restoring $f${reset}"
        dotnet build $f/
        
    fi
done
cp -r $basefolder/src/Presentation/Nop.Web/Plugins/ $basefolder/out
