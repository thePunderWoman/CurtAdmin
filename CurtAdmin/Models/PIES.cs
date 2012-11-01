using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace CurtAdmin.Models {
    public class Report {

        public XDocument generate(int customerID = 1, DateTime? start = null) {
            CurtDevDataContext db = new CurtDevDataContext();
            string AAIAID = "BKDK";
            Dictionary<string,string> pricecodes = new Dictionary<string,string>();
            pricecodes.Add("Jobber","JBR");
            pricecodes.Add("List","LST");
            pricecodes.Add("Map","RMP");

            XDocument xdoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            XElement root = new XElement("PIES");
            //<header>
            //    <PIESVersion> 6.4 </PIESVersion>
            //    <BlanketEffectiveDate> DateTime.Now </BlanketEffectiveDate
            //    <ParentAAIAID> Header.ParentAAIAID </ParentAAIAID>
            //    <BrandOwnerGLN> Header.BrandOwnderGLN </BrandOwnerGLN>
            //    <CurrencyCode> Header.CurrencyCode </CurrencyCode>
            //    <LanguageCode> Header.LanguageCode </LanguageCode>
            //</header>
            XElement header = new XElement("Header",
                new XElement("PIESVersion", 6.4),
                new XElement("BlanketEffectiveDate", String.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                new XElement("ParentAAIAID", AAIAID),
                new XElement("BrandOwnerGLN", "0612314000005"),
                new XElement("CurrencyCode", "USD"),
                new XElement("LanguageCode", "EN"),
                new XElement("TechnicalContact", "Nichole Scott"),
                new XElement("ContactEmail", "websupport@curtmfg.com"));

            //  <Item MaintenanceType="test attribute" >
            //   <HazardousMaterialCode>test string</HazardousMaterialCode>
            //   <PartNumber>test string</PartNumber>
            //   <BrandAAIAID>test string</BrandAAIAID>
            //   <BrandLabel>test string</BrandLabel>
            //   <MinimumOrderQuantity UOM="EA">test string</MinimumOrderQuantity>
            //   <PartTerminologyID>test string</PartTerminologyID>
            XElement items = new XElement("Items");

            List<Part> parts = new List<Part>();
            int partcount = db.CustomerReportParts.Where(x => x.customerID.Equals(customerID)).Select(x => x.partID).Count();

            if (start == null) {
                if (partcount == 0) {
                    parts = db.Parts.OrderBy(x => x.partID).ToList<Part>();
                } else {
                    parts = (from p in db.Parts
                             join crp in db.CustomerReportParts on p.partID equals crp.partID
                             orderby p.partID
                             select p).ToList<Part>();
                }
            } else {
                if (partcount == 0) {
                    parts = db.Parts.Where(x => x.dateModified >= start).OrderBy(x => x.partID).ToList<Part>();
                } else {
                    parts = (from p in db.Parts
                             join crp in db.CustomerReportParts on p.partID equals crp.partID
                             where p.dateModified >= start
                             orderby p.partID
                             select p).ToList<Part>();
                }
            }
            
            foreach (Part part in parts) {
                XElement item = new XElement("Item",
                        new XAttribute("MaintenanceType","A"),
                        new XElement("PartNumber",part.partID),
			            new XElement("HazardousMaterialCode", "N"),
                        new XElement("BrandAAIAID", AAIAID),
                        new XElement("BrandLabel", "CURT"),
                        new XElement("MinimumOrderQuantity",1,
                            new XAttribute("UOM", "EA")),
                        new XElement("Descriptions",
                            new XElement("Description",part.shortDesc,
                                new XAttribute("MaintenanceType","A"),
                                new XAttribute("DescriptionCode","SHO"))),
                        new XElement("Prices", (from p in part.Prices
                                                    select new XElement("Pricing",
                                                        new XAttribute("MaintenanceType", "A"),
                                                        new XAttribute("PriceType", pricecodes[p.priceType]),
                                                        new XElement("EffectiveDate", String.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                                                        new XElement("Price", p.price1.ToString("0.00"))
                                                    )).ToList<XElement>()),
                        new XElement("ProductAttributes", (from pa in part.PartAttributes
                                                                select new XElement("ProductAttribute",pa.field + ": " + pa.value,
                                                                            new XAttribute("MaintenanceType", "A"),
                                                                            new XAttribute("AttrType", "ATR1")
                                                                )).ToList<XElement>(),
                                                          (from cb in part.ContentBridges
                                                           where cb.Content.cTypeID.Equals(2)
                                                           select new XElement("ProductAttribute", cb.Content.text,
                                                                        new XAttribute("MaintenanceType", "A"),
                                                                        new XAttribute("AttrType", "BULL")
                                                           )).ToList<XElement>()),
                        new XElement("Packages", (from pp in part.PartPackages 
                                                    select new XElement("Package",
                                                        new XAttribute("MaintenanceType", "A"),
                                                        new XElement("PackageUOM", pp.packageUnit.code),
                                                        new XElement("QuantityofEaches",pp.quantity),
                                                        new XElement("Dimensions",
                                                            new XAttribute("UOM", pp.dimensionUnit.code),
                                                            new XElement("Height",pp.height),
                                                            new XElement("Width",pp.width),
                                                            new XElement("Length",pp.length)),
                                                        new XElement("Weights",
                                                            new XAttribute("UOM",pp.weightUnit.code),
                                                            new XElement("Weight",pp.weight)
                                                 )))),
                        new XElement("DigitalAssets", (from s in part.ContentBridges
                                                       where s.Content.cTypeID.Equals(5)
                                                       select new XElement("DigitalFileInformation",
                                                           new XAttribute("MaintenanceType","A"),
                                                           new XElement("AssetType","INS"),
                                                           new XElement("FilePath",s.Content.text),
                                                           new XElement("FileName","CURT-"+ part.partID + ".pdf"),
                                                           new XElement("FileType","PDF"),
                                                           new XElement("Representation","A"),
                                                           new XElement("Background","WHI"),
                                                           new XElement("AssetDimensions",
                                                               new XAttribute("UOM","IN"),
                                                               new XElement("AssetHeight",11),
                                                               new XElement("AssetWidth",8.5))
                                                        )).ToList<XElement>(),
                                                      (from pi in part.PartImages
                                                       select new XElement("DigitalFileInformation",
                                                           new XAttribute("MaintenanceType","A"),
                                                           new XElement("AssetType","PO1"),
                                                           new XElement("FilePath",pi.path),
                                                           new XElement("FileName",part.partID + "_" + pi.height + "x" + pi.width + "_" + pi.sort + ".jpg"),
                                                           new XElement("FileType","JPG"),
                                                           new XElement("Representation","A"),
                                                           new XElement("Background","WHI"),
                                                           new XElement("AssetDimensions",
                                                               new XAttribute("UOM","PX"),
                                                               new XElement("AssetHeight",pi.height),
                                                               new XElement("AssetWidth",pi.width))
                                                        )).ToList<XElement>()
                                                    )

                    );


                items.Add(item);
            }

            /*<Pricing MaintenanceType="test attribute"  PriceType="test attribute" >
                    <PriceSheetNumber>test string</PriceSheetNumber>
                    <CurrencyCode>test string</CurrencyCode>
                    <EffectiveDate>2011-01-15</EffectiveDate>
                    <ExpirationDate>2011-01-15</ExpirationDate>
                    <Price>test string</Price>
                    <PriceBreak>test string</PriceBreak>
            </Pricing>*/

    		XElement trailer = new XElement("Trailer",
                           new XElement("ItemCount",parts.Count),
						   new XElement("TransactionDate", String.Format("{0:yyyy-MM-dd}",DateTime.Now)));

            root.Add(header);
            root.Add(items);
            root.Add(trailer);
            xdoc.Add(root);

            return xdoc;
        }

    }
}